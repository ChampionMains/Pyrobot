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

            var rankTask = _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);
            var championMasteriesTask = _riot.GetChampionMasteriesAsync(summoner.Region, summoner.SummonerId);

            var rank = await rankTask;
            var championMasteries = await championMasteriesTask;

            if (rank != null)
                _summoners.UpdateLeagueAsync(summoner, (byte)rank.Item1, rank.Item2);
            _summoners.UpdateChampionMasteriesAsync(championMasteries, summoner);

            await _summoners.SaveChanges();
        }
    }
}