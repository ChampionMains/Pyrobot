using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Util;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    ///     Updates everything that needs to be updated
    /// </summary>
    public class BulkUpdateJob
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly RiotService _riotService;
        private readonly SummonerService _summonerService;

        private readonly RedditService _redditService;
        private readonly FlairService _flairService;
        
        private readonly WebJobConfiguration _config;

        private readonly UnitOfWork _unitOfWork;

        public BulkUpdateJob(RiotService riotService, SummonerService summonerService, RedditService redditService,
            FlairService flairService, WebJobConfiguration config, UnitOfWork unitOfWork)
        {
            _riotService = riotService;
            _summonerService = summonerService;
            _redditService = redditService;
            _flairService = flairService;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.BulkUpdate)] string args)
        {
            if (!await Lock.WaitAsync(1000))
                throw new InvalidOperationException("Lock is not available. Task may already be running.");

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(_config.TimeoutBulkUpdate);

                try
                {
                    await ExecuteInternal(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    throw new TimeoutException($"{nameof(BulkUpdateJob)} timed out ({_config.TimeoutBulkUpdate})", e);
                }
                catch (DbEntityValidationException e)
                {
                    throw new InvalidOperationException(DbValidationExceptionDebugString(e), e);
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

            // TODO: pass along cancellation token to Camille API calls.
            // TODO: different timeout for summoner data and flairs
            await UpdateSummonerData(token);
            await UpdateFlairs(token);
        }

        private async Task UpdateSummonerData(CancellationToken token)
        {
            var summonerSaveBatchSize = _config.BulkUpdateSaveBatchSize;
            var updateBatchSize = _config.BulkUpdateUpdateBatchSize;
            var updateNumBatches = _config.BulkUpdateNumBatches;

            // update summoners
            var summoners = await _summonerService.GetSummonersForUpdateAsync(updateBatchSize, updateNumBatches);

//            var duplicates = summoners
//                .GroupBy(i => new { i.SummonerId, i.Region })
//                .Where(g => g.Count() > 1).ToList();
//            if (duplicates.Count > 0)
//            {
//                throw new InvalidOperationException("Duplicate summonerIds: " + string.Join(",\n", duplicates.Select(g => "[" + string.Join(", ", g) + "]")));
//            }

            Console.Out.WriteLine($"Updating {summoners.Count} summoners.");

            // update each region asynchronously
            var getSummonerTasks = summoners.GroupBy(s => s.Region).Select(async summonersByRegion =>
            {
                var region = summonersByRegion.Key;
                var summonerIdEncs = summonersByRegion.Select(s => s.SummonerIdEnc).ToList();

                if (new HashSet<string>(summonerIdEncs).Count != summonerIdEncs.Count)
                {
                    throw new InvalidOperationException("summonerIdEncs has duplicate values");
                }

                Console.Out.WriteLine($"Updating {summonerIdEncs.Count} summoners from region {region}");

                // arduous async work
                var summonerData = await _riotService.GetSummoners(region, summonerIdEncs);

                if (token.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Updating region {region} cancelled.");
                    return null;
                }
                var summonerRanks = await _riotService.GetRanks(region, summonerIdEncs);
                if (token.IsCancellationRequested)
                {
                    Console.Out.WriteLine($"Updating region {region} cancelled.");
                    return null;
                }
                var summonerMasteries = await _riotService.GetChampionMasteries(region, summonerIdEncs);

                // done with async work
                Console.Out.WriteLine($"Pulled {summonerData.Count} summoner infos, {summonerRanks.Count} ranks, "
                                      + $"and {summonerMasteries.Count} mastery sets for region {region}.");

                return new
                {
                    summonersByRegion,
                    summonerData,
                    summonerRanks,
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
                var summonerRanks = t.summonerRanks;
                var summonerMasteries = t.summonerMasteries;

                var changes =
                    summonersByRegion.Select((summoner, i) => new {summoner, g = i / summonerSaveBatchSize})
                        .GroupBy(x => x.g, x => x.summoner)
                        .Select(summonerBatch =>
                        {
                            foreach (var summoner in summonerBatch)
                            {
                                var data = summonerData[summoner.SummonerIdEnc];
                                Tuple<Tier, Division> rank;
                                summonerRanks.TryGetValue(summoner.SummonerIdEnc, out rank);
                                var mastery = summonerMasteries[summoner.SummonerIdEnc];

                                _summonerService.UpdateSummoner(summoner, region, data.Name, data.ProfileIconId,
                                    rank?.Item1,
                                    (byte?) rank?.Item2, mastery);
                            }

                            // save changes per batch/region
                            // database call
                            _unitOfWork.ChangeTracker.DetectChanges();
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

                try
                {
                    var existingFlairs = await _redditService.GetFlairsAsync(subreddit.Name);

                    Console.Out.WriteLine(
                        $"Pulled {existingFlairs.Count} existing flairs from subreddit {subreddit.Name}.");

                    return new
                    {
                        subreddit,
                        flairsBySubreddit,
                        existingFlairs
                    };
                }
                catch (HttpRequestException e)
                {
                    Console.Out.WriteLine(
                        $"FAILED to pull existing flairs from subreddit {subreddit.Name}.", e);
                    return null;
                }
            }).ToList();

            await Task.WhenAll(getFlairTasks);

            // update flairs in database, generate new flairs to send to reddit
            var setFlairTasks = getFlairTasks.Select(t => t.Result).Select(t =>
            {
                // task cancelled (or propagate null errors)
                if (t == null)
                {
                    // Or failed to pull existing flairs.
                    return null;
                    //if (token.IsCancellationRequested)
                    //    return null;
                    //throw new NullReferenceException("Flair data null.");
                }

                var subreddit = t.subreddit;
                var flairsBySubreddit = t.flairsBySubreddit;
                var existingFlairs = t.existingFlairs;

                var updatedFlairs = flairsBySubreddit.Select(flair =>
                {
                    var existingFlair = existingFlairs.FirstOrDefault(
                        ef => string.Equals(ef.User, flair.User.Name, StringComparison.OrdinalIgnoreCase));

                    // update database flair text from subreddit (different from individual flair update service)
                    if (existingFlair != null)
                    {
                        // sanitize if the flair has the mastery text class to extract just the text portion
                        flair.FlairText = existingFlair.FlairText != null &&
                            (existingFlair.FlairCssClass?.Contains(FlairService.MasteryTextClass) ?? false)
                            ? FlairUtil.SanitizeFlairTextLeadingMastery(existingFlair.FlairText)
                            : existingFlair.FlairText;
                    }
                    flair.LastUpdate = DateTimeOffset.Now;

                    var userSummoners = _summonerService.GetSummonersIncludeDataByUserId(flair.UserId);

                    var newFlair = _flairService.GenerateFlair(flair.User.Name, userSummoners, subreddit,
                        flair.RankEnabled, flair.ChampionMasteryEnabled,
                        flair.PrestigeEnabled, flair.ChampionMasteryTextEnabled, flair.FlairText,
                        existingFlair?.FlairCssClass);

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

            _unitOfWork.ChangeTracker.DetectChanges();
            var flairUpdates = await _flairService.SaveChangesAsync();
            Console.Out.WriteLine($"Updating flairs {(token.IsCancellationRequested ? "interrupted" : "complete")}, {flairUpdates} rows affected.");

            token.ThrowIfCancellationRequested();
        }

        private string DbValidationExceptionDebugString(DbEntityValidationException e)
        {
            var writer = new StringWriter();
            writer.WriteLine("DbEntityValidationException ------------");
            foreach (var entity in e.EntityValidationErrors)
            {
                writer.WriteLine("  " + entity.Entry.Entity + ":");
                foreach (var error in entity.ValidationErrors)
                {
                    writer.WriteLine("    " + error.PropertyName + ": " + error.ErrorMessage);
                }
            }
            writer.WriteLine("----------------------------------------");
            return writer.ToString();
        }
    }
}
