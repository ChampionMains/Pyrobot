using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJobModels;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob
{
    public class Functions
    {
        private readonly RiotService _riot;
        private readonly SummonerService _summoners;
        private readonly UserService _users;

        public Functions(UserService users, RiotService riot, SummonerService summoners)
        {
            _users = users;
            _riot = riot;
            _summoners = summoners;
        }

        public async Task<bool> SummonerUpdate([QueueTrigger("SummonerUpdate")] int id)
        {
            var summoner = await _summoners.FindAsync(id);
            if (summoner == null)
                return false;

            var leagues = await _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);
            var solo = leagues?.FirstOrDefault(league => league.Queue == QueueType.RANKED_SOLO_5x5);

            if (solo == null)
            {
                await _summoners.UpdateLeagueAsync(summoner, (byte)Tiers.Unranked, 0);
            }
            else
            {
                var entry = solo.Entries.First(e => e.PlayerOrTeamId == summoner.SummonerId.ToString());
                var division = (byte)entry.Division;
                var tier = (Tiers)Enum.Parse(typeof(Tiers), solo.Tier.ToString(), true);
                await _summoners.UpdateLeagueAsync(summoner, (byte)tier, division);
            }
            return true;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("log")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }
    }
}
