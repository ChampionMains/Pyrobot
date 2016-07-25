using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

namespace ChampionMains.Pyrobot.Jobs
{
    /// <summary>
    ///     Updates the league standing for a summoner.
    /// </summary>
    public class BulkUpdateJob
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly RiotService _riotService;
        private readonly SummonerService _summonersService;

        public BulkUpdateJob(RiotService riotService, SummonerService summonersService)
        {
            _riotService = riotService;
            _summonersService = summonersService;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.BulkUpdate)] string args)
        {
            if (!await Lock.WaitAsync(1000))
                throw new InvalidOperationException("Lock is not available. Task is already be running.");

            try
            {
                await ExecuteInternal();
            }
            finally
            {
                Lock.Release();
            }
        }

        private async Task ExecuteInternal()
        {
            // update summoners
            var summoners = await _summonersService.GetSummonersForUpdateAsync();

            foreach (var summonersByRegion in summoners.GroupBy(s => s.Region, s => s))
            {
                var region = summonersByRegion.Key;
                var summonerIds = summonersByRegion.Select(s => s.SummonerId).ToList();
                var summonerData = (await _riotService.GetSummoners(region, summonerIds)).ToDictionary(s => s.SummonerId, s => s);
                var summonerRanks = await _riotService.GetRanks(region, summonerIds);
                var summonerMasteries = await _riotService.GetChampionMasteries(region, summonerIds);

                foreach (var summoner in summoners)
                {
                    var data = summonerData[summoner.SummonerId];
                    var rank = summonerRanks[summoner.SummonerId];
                    var mastery = summonerMasteries[summoner.SummonerId];
                    _summonersService.UpdateSummoner(summoner, region, data.Name, data.ProfileIconId, rank.Item1, rank.Item2, mastery);
                }
            }

            await _summonersService.SaveChangesAsync();


            //var summoner = await _summonersService.FindSummonerAsync(id);
            //if (summoner == null)
            //    return;

            //var summoner2 = await _riotService.GetSummoner(summoner.Region, summoner.SummonerId);
            //var rank = await _riotService.GetRank(summoner.Region, summoner.SummonerId);
            //var championMasteries = await _riotService.GetChampionMastery(summoner.Region, summoner.SummonerId);

            //if (rank != null)
            //    _summonersService.UpdateLeagueAsync(summoner, (byte)rank.Item1, rank.Item2);
            //_summonersService.UpdateChampionMasteriesAsync(championMasteries, summoner);
            //// calls savechanges
            //await _summonersService.AddOrUpdateSummonerAsync(summoner.User, summoner.SummonerId, summoner.Region,
            //    summoner2.Name, summoner2.ProfileIconId);

            //await _summonersService.SaveChanges();
        }
    }
}