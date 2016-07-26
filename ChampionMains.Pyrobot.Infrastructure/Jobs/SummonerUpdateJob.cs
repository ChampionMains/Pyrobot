using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

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
            var summoner = await _summoners.FindSummonerAsync(id);
            if (summoner == null)
                return;

            var summoner2 = await _riot.GetSummoner(summoner.Region, summoner.SummonerId);
            var rank = await _riot.GetRank(summoner.Region, summoner.SummonerId);
            var championMasteries = await _riot.GetChampionMastery(summoner.Region, summoner.SummonerId);

            _summoners.UpdateSummoner(summoner, summoner.Region, summoner.Name, summoner2.ProfileIconId,
                rank?.Item1, rank?.Item2, championMasteries);
            await _summoners.SaveChangesAsync();
        }
    }
}