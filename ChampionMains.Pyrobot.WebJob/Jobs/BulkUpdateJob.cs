using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Util;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    ///     Updates the league standing for a summoner.
    /// </summary>
    public class BulkUpdateJob
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly RiotService _riotService;
        private readonly SummonerService _summonerService;

        private readonly RedditService _redditService;
        private readonly FlairService _flairService;

        private readonly TimeSpan _timeout;

        public BulkUpdateJob(RiotService riotService, SummonerService summonerService,
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
                var task = ExecuteInternal();
                var result = await Task.WhenAny(task, Task.Delay(_timeout));

                if (result != task)
                    throw new TimeoutException($"{nameof(BulkUpdateJob)} timed out ({_timeout})");

                if (result.Exception != null)
                    throw result.Exception;
            }
            finally
            {
                Lock.Release();
            }
        }

        private async Task ExecuteInternal()
        {
            const int summonerSaveBatchSize = 400;

            // update summoners
            var summoners = await _summonerService.GetSummonersForUpdateAsync();

            Console.Out.WriteLine($"Updating {summoners.Count} summoners.");

            // update each region asynchronously
            var summonerTasks = summoners.GroupBy(s => s.Region, s => s).Select(async summonersByRegion =>
            {
                var region = summonersByRegion.Key;
                var summonerIds = summonersByRegion.Select(s => s.SummonerId).ToList();

                Console.Out.WriteLine($"Updating {summonerIds.Count} summoners from region {region}");

                var summonerData = await _riotService.GetSummoners(region, summonerIds);
                var summonerRanks = await _riotService.GetRanks(region, summonerIds);
                var summonerMasteries = await _riotService.GetChampionMasteries(region, summonerIds);

                Console.Out.WriteLine($"Pulled {summonerData.Count} summoner infos, {summonerRanks.Count} ranks, "
                                      + $"and {summonerMasteries.Count} mastery sets for region {region}.");

                // nothing async below here
                // save summoners in batches
                var changes = summonersByRegion
                    .Select((summoner, i) => new {summoner, g = i / summonerSaveBatchSize}).GroupBy(x => x.g, x => x.summoner)
                    .Select(summonerBatch =>
                    {
                        foreach (var summoner in summonersByRegion)
                        {
                            var data = summonerData[summoner.SummonerId];
                            Tuple<Tier, byte> rank;
                            summonerRanks.TryGetValue(summoner.SummonerId, out rank);
                            var mastery = summonerMasteries[summoner.SummonerId];

                            _summonerService.UpdateSummoner(summoner, region, data.Name, data.ProfileIconId, rank?.Item1,
                                rank?.Item2, mastery);
                        }

                        // save changes per batch/region
                        return _summonerService.SaveChanges();
                    }).Sum();

                Console.Out.WriteLine($"Completed updating {region}.");

                return changes;
            }).ToList();

            await Task.WhenAll(summonerTasks);

            var summonerUpdates = summonerTasks.Select(t => t.Result).Sum();
            Console.Out.WriteLine($"Updating summoners complete, {summonerUpdates} rows affected.");

            // update flairs
            var flairs = await _flairService.GetFlairsForUpdateAsync();
            Console.Out.WriteLine($"Updating {flairs.Count} flairs.");

            // pull existing flairs
            var getFlairTasks = flairs.GroupBy(f => f.SubredditId, f => f).Select(async flairsBySubreddit =>
            {
                var subreddit = _flairService.GetSubreddit(flairsBySubreddit.Key);

                Console.Out.WriteLine($"Updating flairs from subreddit {subreddit.Name}.");

                var existingFlairs = await _redditService.GetFlairsAsync(subreddit.Name);
                
                Console.Out.WriteLine($"Pulled {existingFlairs.Count} existing flairs from subreddit {subreddit.Name}.");

                //return Tuple.Create(subreddit, flairsBySubreddit, existingFlairs);
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

            }).Select(async x => await _redditService.SetFlairsAsync(x.subreddit.Name, x.updatedFlairs));

            await Task.WhenAll(setFlairTasks);

            var flairUpdates = await _flairService.SaveChangesAsync();
            Console.Out.WriteLine($"Updating flairs complete, {flairUpdates} rows affected.");
        }
    }
}