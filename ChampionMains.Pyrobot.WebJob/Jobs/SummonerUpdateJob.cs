using System;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    ///     Updates the league standing for a summoner.
    /// </summary>
    public class SummonerUpdateJob
    {
        private readonly RiotService _riot;
        private readonly SummonerService _summoners;

        private readonly TimeSpan _riotUpdateMinStaleTime;

        public SummonerUpdateJob(RiotService riot, SummonerService summoners, ApplicationConfiguration config)
        {
            _riot = riot;
            _summoners = summoners;

            _riotUpdateMinStaleTime = config.RiotUpdateMin;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.SummonerUpdate)] int id)
        {
            var summoner = _summoners.FindSummonerIncludeRankAndChampionMasteries(id);
            if (null == summoner)
                return;

            if (summoner.LastUpdate > DateTimeOffset.Now - _riotUpdateMinStaleTime)
                return;

            var summonerData = await _riot.GetSummoner(summoner.Region, summoner.SummonerIdEnc);

            var rank = await _riot.GetRank(summoner.Region, summoner.SummonerIdEnc);
            var championMasteries = await _riot.GetChampionMastery(summoner.Region, summoner.SummonerIdEnc);

            _summoners.UpdateSummoner(summoner, summoner.Region, summonerData.Name, summonerData.ProfileIconId,
                rank?.Item1, (byte?) rank?.Item2, championMasteries);
            await _summoners.SaveChangesAsync();
        }
    }
}