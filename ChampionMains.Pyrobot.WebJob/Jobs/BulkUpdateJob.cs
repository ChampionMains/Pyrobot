using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Startup;
using ChampionMains.Pyrobot.Util;
using Microsoft.Azure.WebJobs;
using RiotSharp;
using RiotSharp.LeagueEndpoint;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    ///     Updates everything that needs to be updated
    /// </summary>
    public class BulkUpdateJob
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly RiotApi _riotService;
        private readonly SummonerService _summonerService;

        private readonly RedditService _redditService;
        private readonly FlairService _flairService;

        private readonly TimeSpan _timeout;

        public BulkUpdateJob(RiotApi riotService, SummonerService summonerService,
            RedditService redditService, FlairService flairService, WebJobConfiguration config)
        {
            _riotService = riotService;
            _summonerService = summonerService;
            _redditService = redditService;
            _flairService = flairService;
            _timeout = config.TimeoutBulkUpdate;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.BulkUpdate)] string args)
        {
            if (!await Lock.WaitAsync(1000))
                throw new InvalidOperationException("Lock is not available. Task is already be running.");

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(_timeout);

                try
                {
                    await ExecuteInternal(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    throw new TimeoutException($"{nameof(BulkUpdateJob)} timed out ({_timeout})", e);
                }
            }
            finally
            {
                Lock.Release();
            }
        }

        private async Task ExecuteInternal(CancellationToken token)
        {
            // cancellation only occurs during the http api calls.
            // once cancelled, the task no longer issues more http api calls,
            // but will wait for existing calls to finish,
            // and will continue to save the updated data into the database.

            // TODO: different timeout for summoner data and flairs
            await UpdateSummonerData(token);
            await UpdateFlairs(token);
        }

        private async Task UpdateSummonerData(CancellationToken token)
        {
            const int summonerSaveBatchSize = 400;

            // update summoners
            var summoners = await _summonerService.GetSummonersForUpdateAsync();

            var duplicates = summoners.GroupBy(i => new { i.SummonerId, i.Region })
                .Where(g => g.Count() > 1).ToList();
            if (duplicates.Count > 0)
            {
                throw new InvalidOperationException("Duplicate summonerIds: " + string.Join(", ", duplicates.Select(g => "[" + string.Join(", ", g))));
            }

            Console.Out.WriteLine($"Updating {summoners.Count} summoners.");

            // update each region asynchronously
            var getSummonerTasks = summoners.GroupBy(s => s.Region).Select(async summonersByRegion =>
            {
                RiotSharp.Region region;
                if (!Enum.TryParse(summonersByRegion.Key.ToLowerInvariant(), out region))
                    throw new InvalidOperationException("Bad region encountered while updating: " + summonersByRegion.Key);
                var summonerIds = summonersByRegion.Select(s => s.SummonerId).ToList();

                if (new HashSet<long>(summonerIds).Count != summonerIds.Count)
                {
                    throw new InvalidOperationException("summonerIds has duplicate values");
                }

                Console.Out.WriteLine($"Updating {summonerIds.Count} summoners from region {region}");

                // arduous aync work
                // general data
                var summonerData = (await _riotService.GetSummonersAsync(region, summonerIds)).ToDictionary(s => s.Id, s => s);
                if (token.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Updating region {region} cancelled.");
                    return null;
                }
                // leauges
                Dictionary<long, List<League>> summonerLeagues = null;
                try
                {
                    summonerLeagues = await _riotService.GetLeaguesAsync(region, summonerIds);
                }
                catch (RiotSharpException e)
                {
                    if (!e.Message.StartsWith("404")) // indicates summoners are unranked
                        throw;
                }
                if (token.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Updating region {region} cancelled.");
                    return null;
                }
                // champion mastery
                var summonerMasteryTasks = summonerIds.ToDictionary(s => s, s => _riotService.GetAllChampionsMasteryEntriesAsync(
                    RegionPlatformUtil.ConvertRegionToPlatform(region), s));
                await Task.WhenAll(summonerMasteryTasks.Values);
                var summonerMasteries = summonerMasteryTasks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result);

                // done with async work
                Console.Out.WriteLine($"Pulled {summonerData.Count} summoner infos, {summonerLeagues.Count} leagues, "
                                      + $"and {summonerMasteries.Count} mastery sets for region {region}.");

                return new
                {
                    summonersByRegion,
                    summonerData,
                    summonerLeagues,
                    summonerMasteries
                };
            }).ToList();

            await Task.WhenAll(getSummonerTasks);

            var summonerUpdates = getSummonerTasks.Select(t => t.Result).Select(t =>
            {
                // task cancelled (or propagate null errors)
                if (t == null && token.IsCancellationRequested)
                    return 0;

                var summonersByRegion = t.summonersByRegion;
                var region = summonersByRegion.Key;

                var summonerData = t.summonerData;
                var summonerLeagues = t.summonerLeagues;
                var summonerMasteries = t.summonerMasteries;

                var changes =
                    summonersByRegion.Select((summoner, i) => new {summoner, g = i/summonerSaveBatchSize})
                        .GroupBy(x => x.g, x => x.summoner)
                        .Select(summonerBatch =>
                        {
                            foreach (var summoner in summonersByRegion)
                            {
                                var data = summonerData[summoner.SummonerId];

                                List<League> leagues;
                                summonerLeagues.TryGetValue(summoner.SummonerId, out leagues);
                                var rank = RankUtil.GetHighestLeague(leagues, summoner.SummonerId);

                                var mastery = summonerMasteries[summoner.SummonerId];

                                _summonerService.UpdateSummoner(summoner, region, data.Name, data.ProfileIconId,
                                    rank.Item1, rank.Item2, mastery);
                            }

                            // save changes per batch/region
                            // database call
                            return _summonerService.SaveChanges();
                        }).Sum();

                Console.Out.WriteLine($"Completed updating {region}.");

                return changes;
            }).Sum();

            Console.Out.WriteLine($"Updating summoners {(token.IsCancellationRequested ? "interrupted" : "complete")}, {summonerUpdates} rows affected.");
            token.ThrowIfCancellationRequested();
        }

        private async Task UpdateFlairs(CancellationToken token)
        {
            // update flairs
            var flairs = await _flairService.GetFlairsForUpdateAsync();
            Console.Out.WriteLine($"Updating {flairs.Count} flairs.");

            // pull existing flairs
            var getFlairTasks = flairs.GroupBy(f => f.SubredditId, f => f).Select(async flairsBySubreddit =>
            {
                // cancel if need
                if (token.IsCancellationRequested)
                    return null;

                var subreddit = _flairService.GetSubreddit(flairsBySubreddit.Key);

                Console.Out.WriteLine($"Updating flairs from subreddit {subreddit.Name}.");

                var existingFlairs = await _redditService.GetFlairsAsync(subreddit.Name);

                Console.Out.WriteLine($"Pulled {existingFlairs.Count} existing flairs from subreddit {subreddit.Name}.");

                return new
                {
                    subreddit,
                    flairsBySubreddit,
                    existingFlairs
                };
            }).ToList();

            await Task.WhenAll(getFlairTasks);

            // update flairs in database, generate new flairs to send to reddit
            var setFlairTasks = getFlairTasks.Select(t => t.Result).Select(t =>
            {
                // task cancelled (or propagate null errors)
                if (t == null && token.IsCancellationRequested)
                    return null;

                var subreddit = t.subreddit;
                var flairsBySubreddit = t.flairsBySubreddit;
                var existingFlairs = t.existingFlairs;

                var updatedFlairs = flairsBySubreddit.Select(flair =>
                {
                    var existingFlair = existingFlairs.FirstOrDefault(
                        ef => string.Equals(ef.Name, flair.User.Name, StringComparison.OrdinalIgnoreCase));

                    // update database flair text from subreddit (different from individual flair update service)
                    if (existingFlair != null)
                    {
                        // sanitize if the flair has the mastery text class to extract just the text portion
                        flair.FlairText = existingFlair.Text != null &&
                            (existingFlair.CssClass?.Contains(FlairService.MasteryTextClass) ?? false)
                            ? FlairUtil.SanitizeFlairTextLeadingMastery(existingFlair.Text)
                            : existingFlair.Text;
                    }
                    flair.LastUpdate = DateTimeOffset.Now;

                    var userSummoners = _summonerService.GetSummonersIncludeDataByUserId(flair.UserId);

                    var newFlair = _flairService.GenerateFlair(flair.User.Name, userSummoners, subreddit,
                        flair.RankEnabled, flair.ChampionMasteryEnabled,
                        flair.PrestigeEnabled, flair.ChampionMasteryTextEnabled, flair.FlairText,
                        existingFlair?.CssClass);

                    return newFlair;
                }).ToList();

                return new
                {
                    subreddit,
                    updatedFlairs
                };

            }).Select(async x =>
            {
                // cancel if x is null OR task cancelled
                if (x == null || token.IsCancellationRequested)
                    return;

                await _redditService.SetFlairsAsync(x.subreddit.Name, x.updatedFlairs);
            });

            await Task.WhenAll(setFlairTasks);

            var flairUpdates = await _flairService.SaveChangesAsync();
            Console.Out.WriteLine($"Updating flairs {(token.IsCancellationRequested ? "interrupted" : "complete")}, {flairUpdates} rows affected.");

            token.ThrowIfCancellationRequested();
        }
    }
}
