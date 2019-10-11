using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Util;
using Microsoft.Azure.WebJobs;
using Reddit.Things;

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
            // Cancellation only occurs during the http api calls.
            // Once cancelled, the task will no longer issues more http API calls,
            // but will wait for existing calls to finish,
            // and will continue to save the updated data into the database.
            
            // TODO: different timeout for summoner data and flairs

            var updateSubredditMissingAdminTask = UpdateSubredditMissinAdmin(token);
            var updateSummonerDataTask = UpdateSummonerData(token);

            await updateSubredditMissingAdminTask;
            await updateSummonerDataTask;

            await UpdateFlairs(token);
        }

        private async Task UpdateSubredditMissinAdmin(CancellationToken token)
        {
            var modSubreddits = await _redditService.GetModSubredditsAsync();

            // Use separate unit of work.
            // TODO fix how unitofwork is used everywhere.
            using (var unitOfWork = new UnitOfWork())
            {
                var dbSubreddits = await unitOfWork.Subreddits.ToListAsync(token);
                foreach (var dbSubreddit in dbSubreddits)
                {
                    dbSubreddit.MissingMod = !modSubreddits.Contains(dbSubreddit.Name);
                    if (dbSubreddit.MissingMod)
                        Console.Out.WriteLine($"Bot missing mod on subreddit: {dbSubreddit.Name}.");
                }
                await unitOfWork.SaveChangesAsync(token);
            }
        }

        private async Task UpdateSummonerData(CancellationToken token)
        {
            var summonerSaveBatchSize = _config.BulkUpdateSaveBatchSize;
            var updateBatchSize = _config.BulkUpdateUpdateBatchSize;
            var updateNumBatches = _config.BulkUpdateNumBatches;

            // update summoners
            var summoners = await _summonerService.GetSummonersForUpdateAsync(updateBatchSize, updateNumBatches);

            Console.Out.WriteLine($"Updating {summoners.Count} summoners.");

            // Update each region asynchronously (concurrent).
            var getSummonerTasks = summoners.GroupBy(s => s.Region).Select(async summonersByRegion =>
            {
                var region = summonersByRegion.Key;
                var summonerIdEncs = summonersByRegion.Select(s => s.SummonerIdEnc).ToList();

                var duplicate = summonerIdEncs
                    .Where(s => !string.IsNullOrEmpty(s))
                    .GroupBy(s => s)
                    .FirstOrDefault(g => g.Count() > 1)?.Key;
                if (duplicate != null)
                    throw new InvalidOperationException("summonerIdEncs has duplicate: " + duplicate);

                Console.Out.WriteLine($"Updating {summonerIdEncs.Count} summoners from region {region}");

                // Arduous async work (concurrent).
                var summonerDataTask = _riotService.GetSummoners(region, summonerIdEncs, token);
                var summonerRanksTask = _riotService.GetRanks(region, summonerIdEncs, token);
                var summonerMasteriesTask = _riotService.GetChampionMasteries(region, summonerIdEncs, token);
                // Await results.
                var summonerData = await summonerDataTask;
                var summonerRanks = await summonerRanksTask;
                var summonerMasteries = await summonerMasteriesTask;

                // Done with async work.
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
                // Task cancelled (or throw for null error).
                if (t == null)
                {
                    if (token.IsCancellationRequested)
                        return 0;
                    throw new ArgumentException("Summoner data is null.");
                }

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
            // Update flairs.
//            var flairs = await _flairService.GetFlairsForUpdateAsync(token);

            // Update by subreddit to batch Reddit API calls.
            var subredditsNeedingFlairUpdate = await
                _flairService.GetSubredditsForFlairUpdateAsync(_config.UpdateFlairsNumSubreddits, token);

            {
                var totalFlairs = subredditsNeedingFlairUpdate.Select(s => s.SubredditUserFlairs.Count).Sum();
                Console.Out.WriteLine($"Updating {totalFlairs} flairs.");
            }

            // Pull existing flairs.
            var existingFlairDataTasks = subredditsNeedingFlairUpdate.Select(async subreddit =>
            {
                Console.Out.WriteLine($"Updating flairs from subreddit {subreddit.Name}.");

                try
                {
                    var existingFlairs = await _redditService.GetFlairsAsync(subreddit.Name);

                    Console.Out.WriteLine(
                        $"Pulled {existingFlairs.Count} existing flairs from subreddit {subreddit.Name}.");

                    return new
                    {
                        subreddit,
                        existingFlairs
                    };
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(
                        $"FAILED to pull existing flairs from subreddit {subreddit.Name}.", e);
                    return null;
                }
            }).ToList();

            var existingFlairDatas = await Task.WhenAll(existingFlairDataTasks);

            if (token.IsCancellationRequested)
            {
                Console.Out.WriteLine("Updating flairs interrupted early, no flairs updated.");
                return;
            }

            // Update flairs in database, generate new flairs to send to Reddit.
            var setFlairTasks = existingFlairDatas.Select(t =>
            {
                // Task cancelled or failed to pull existing flairs.
                if (t == null)
                    return null;

                var subreddit = t.subreddit;
                var existingFlairs = t.existingFlairs;

                var groupUnchanged = subreddit.SubredditUserFlairs.Select(dbFlair =>
                {
                    var existingFlairFromReddit = existingFlairs.FirstOrDefault(
                        ef => string.Equals(ef.User, dbFlair.User.Name, StringComparison.OrdinalIgnoreCase));

                    // Update database flair text from subreddit (different from individual flair update service).
                    if (existingFlairFromReddit != null)
                    {
                        // If user cleared their flair text, clear the text in the db row.
                        if (null == existingFlairFromReddit.FlairText)
                        {
                            dbFlair.FlairText = null;
                        }
                        // Otherwise, update the db flair text from the reddit flair data.
                        else
                        {
                            // Sanitize if the flair has the mastery text class to extract just the text portion.
                            dbFlair.FlairText =
                            (existingFlairFromReddit.FlairCssClass?.Contains(FlairService.MasteryTextClass) ??
                             false)
                                ? FlairUtil.SanitizeFlairTextLeadingMastery(existingFlairFromReddit.FlairText)
                                : existingFlairFromReddit.FlairText;
                            // Decode HTML entities. TODO: https://www.reddit.com/r/bugs/comments/cyz8x1/
                            // Loop due to entities building up: "&amp;amp;amp;amp;lt;3". TODO: remove loop.
                            while (true)
                            {
                                var newText = HttpUtility.HtmlDecode(dbFlair.FlairText);
                                if (dbFlair.FlairText == newText)
                                    break;
                                dbFlair.FlairText = newText;
                            }

                            if (dbFlair.FlairText?.Length > 64)
                                throw new InvalidOperationException(
                                    $"Flair text too long: \"{dbFlair.FlairText}\", " +
                                    $"Id: {dbFlair.Id}, Subreddit: {subreddit.Name}, User: {dbFlair.User.Name}.");
                        }
                    }

                    // Update flair timestamp.
                    dbFlair.LastUpdate = DateTimeOffset.Now;

                    var userSummoners = _summonerService.GetSummonersIncludeDataByUserId(dbFlair.UserId);

                    var newFlair = FlairService.GenerateFlair(dbFlair.User.Name, userSummoners, subreddit,
                        dbFlair.RankEnabled, dbFlair.ChampionMasteryEnabled,
                        dbFlair.PrestigeEnabled, dbFlair.ChampionMasteryTextEnabled, dbFlair.FlairText,
                        existingFlairFromReddit?.FlairCssClass);

                    if (FlairService.IsFlairUnchanged(existingFlairFromReddit, newFlair))
                        return null;

                    return newFlair;

                }).GroupBy(x => null == x).ToDictionary(g => g.Key, g => g.ToList());

                List<FlairListResult> updatedFlairs, unchangedFlairs;
                groupUnchanged.TryGetValue(false, out updatedFlairs);
                groupUnchanged.TryGetValue(true, out unchangedFlairs);

                {
                    int updated = updatedFlairs?.Count ?? 0;
                    int unchanged = unchangedFlairs?.Count ?? 0;
                    int total = updated + unchanged;
                    Console.WriteLine($@"Computed flairs, {updated} updated out of {total} total ({updated * 100.0 / total}%).");
                }

                // Update subreddit timestamp.
                subreddit.LastBulkUpdate = DateTimeOffset.Now;

                return new
                {
                    subreddit,
                    updatedFlairs
                };

            }).Select(async x =>
            {
                // Cancel if x is null OR task cancelled.
                if (x == null || token.IsCancellationRequested)
                    return;

                var oks = await _redditService.SetFlairsAsync(x.subreddit.Name, x.updatedFlairs);
                Console.WriteLine($@"Updated {oks} out of {x.updatedFlairs.Count} flairs for subreddit {x.subreddit.Name}.");
            });

            await Task.WhenAll(setFlairTasks);

            _unitOfWork.ChangeTracker.DetectChanges();
            var flairUpdates = await _flairService.SaveChangesAsync();
            Console.Out.WriteLine($"Updating flairs {(token.IsCancellationRequested ? "interrupted" : "complete")}, {flairUpdates} rows affected.");

            token.ThrowIfCancellationRequested();
        }

        private static string DbValidationExceptionDebugString(DbEntityValidationException e)
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
