using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Startup;
using Microsoft.Azure.WebJobs;
using RiotSharp;
using RiotSharp.LeagueEndpoint;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    ///     Updates the league standing for a summoner.
    /// </summary>
    public class SummonerUpdateJob
    {
        private readonly RiotApi _riot;
        private readonly SummonerService _summoners;

        private readonly TimeSpan _riotUpdateMinStaleTime;

        public SummonerUpdateJob(RiotApi riot, SummonerService summoners, ApplicationConfiguration config)
        {
            _riot = riot;
            _summoners = summoners;

            _riotUpdateMinStaleTime = config.RiotUpdateMin;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.SummonerUpdate)] int id)
        {
            var summoner = _summoners.FindSummonerIncludeRankAndChampionMasteries(id);
            if (summoner == null)
                return;

            if (summoner.LastUpdate > DateTimeOffset.Now - _riotUpdateMinStaleTime)
                return;

            RiotSharp.Region region;
            if (!Enum.TryParse(summoner.Region.ToLowerInvariant(), out region))
                throw new InvalidOperationException("Bad region: " + summoner.Region);

            var summonerData = await _riot.GetSummonerAsync(region, summoner.SummonerId);

            Dictionary<long, List<League>> summonerLeagues = null;
            try
            {
                summonerLeagues = await _riot.GetLeaguesAsync(region, new List<long>(1) {summoner.SummonerId});
            }
            catch (RiotSharpException e)
            {
                if (!e.Message.StartsWith("404")) // indicates summoner is unranked
                    throw;
            }
            List<League> leagues = null;
            summonerLeagues?.TryGetValue(summoner.SummonerId, out leagues);
            var rank = RankUtil.GetHighestLeague(leagues, summoner.SummonerId);
            
            var championMasteries = await _riot.GetAllChampionsMasteryEntriesAsync(RegionPlatformUtil.ConvertRegionToPlatform(region), summoner.SummonerId);

            _summoners.UpdateSummoner(summoner, summoner.Region, summonerData.Name, summonerData.ProfileIconId,
                rank.Item1, rank.Item2, championMasteries);
            await _summoners.SaveChangesAsync();
        }
    }
}