using System;
using System.Linq;
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
    public class SummonerUpdateJob
    {
        private readonly RiotService _riot;
        private readonly SummonerService _summoners;

        public SummonerUpdateJob(RiotService riot, SummonerService summoners)
        {
            _riot = riot;
            _summoners = summoners;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.SummonerUpdate)] int id)
        {
            var summoner = await _summoners.FindAsync(id);
            if (summoner == null)
                return;

            var summoner2 = await _riot.FindSummonerAsync(summoner.Region, summoner.SummonerId);
            var rank = await _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);
            var championMasteries = await _riot.GetChampionMasteriesAsync(summoner.Region, summoner.SummonerId);

            if (rank != null)
                _summoners.UpdateLeagueAsync(summoner, (byte) rank.Item1, rank.Item2);
            _summoners.UpdateChampionMasteriesAsync(championMasteries, summoner);
            // calls savechanges
            await _summoners.AddOrUpdateSummonerAsync(summoner.User, summoner.SummonerId, summoner.Region,
                summoner2.Name, summoner2.ProfileIconId);

            //await _summoners.SaveChanges();
        }
    }
}