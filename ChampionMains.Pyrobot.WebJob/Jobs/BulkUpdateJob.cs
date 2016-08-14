using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Services;
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
                if (await Task.WhenAny(task, Task.Delay(_timeout)) != task)
                {
                    throw new TimeoutException($"{nameof(BulkUpdateJob)} timed out ({_timeout})");
                }
            }
            finally
            {
                Lock.Release();
            }
        }

        private async Task ExecuteInternal()
        {
            // update summoners
            var summoners = await _summonerService.GetSummonersForUpdateAsync();
            await Console.Out.WriteLineAsync($"Updating {summoners.Count} summoners...");

            foreach (var summonersByRegion in summoners.GroupBy(s => s.Region, s => s))
            {
                var region = summonersByRegion.Key;
                var summonerIds = summonersByRegion.Select(s => s.SummonerId).ToList();
                var summonerData = await _riotService.GetSummoners(region, summonerIds);
                var summonerRanks = await _riotService.GetRanks(region, summonerIds);
                var summonerMasteries = await _riotService.GetChampionMasteries(region, summonerIds);

                foreach (var summoner in summonersByRegion)
                {
                    var data = summonerData[summoner.SummonerId];
                    Tuple<Tier, byte> rank;
                    summonerRanks.TryGetValue(summoner.SummonerId, out rank);
                    var mastery = summonerMasteries[summoner.SummonerId];

                    _summonerService.UpdateSummoner(summoner, region, data.Name, data.ProfileIconId, rank?.Item1, rank?.Item2, mastery);
                }
            }

            var summonerUpdates = await _summonerService.SaveChangesAsync();
            await Console.Out.WriteLineAsync($"{summonerUpdates} rows affected.");


            // update flairs
            var flairs = await _flairService.GetFlairsForUpdateAsync();
            await Console.Out.WriteLineAsync($"Updating {flairs.Count} flairs...");

            var flairTasks = flairs.GroupBy(f => f.SubredditId, f => f).Select(async flairsBySubreddit =>
            {
                var subreddit = flairsBySubreddit.First().Subreddit;

                var existingFlairs = await _redditService.GetFlairsAsync(subreddit.Name);

                var updatedFlairs = flairsBySubreddit.Select(flair =>
                {
                    var existingFlair = existingFlairs.FirstOrDefault(
                        ef => string.Equals(ef.Name, flair.User.Name, StringComparison.OrdinalIgnoreCase)) ??
                        new UserFlairParameter
                        {
                            Name = flair.User.Name,
                            Text = flair.FlairText
                        };

                    // update database flair text from subreddit (different from individual flair update service)
                    flair.FlairText = existingFlair.Text;
                    flair.LastUpdate = DateTimeOffset.Now;

                    var classes = _flairService.GenerateFlairCSS(flair.UserId, subreddit.ChampionId, subreddit.RankEnabled && flair.RankEnabled,
                        subreddit.ChampionMasteryEnabled && flair.ChampionMasteryEnabled,
                        subreddit.PrestigeEnabled && flair.PrestigeEnabled, existingFlair.CssClass);
                    existingFlair.CssClass = classes;
                    return existingFlair;
                }).ToList();

                return await _redditService.SetFlairsAsync(subreddit.Name, updatedFlairs);
            }).ToList();

            await Task.WhenAll(flairTasks);

            var flairUpdates = await _flairService.SaveChangesAsync();
            await Console.Out.WriteLineAsync($"{flairUpdates} rows affected.");
        }
    }
}