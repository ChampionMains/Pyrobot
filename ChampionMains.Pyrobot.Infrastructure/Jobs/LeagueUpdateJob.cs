using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;
using Hallam.RedditRankedFlairs.Riot;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs.Jobs
{
    /// <summary>
    ///     Updates the league standing for a summoner.
    /// </summary>
    public class LeagueUpdateJob
    {
        private readonly IRiotService _riot;
        private readonly ISummonerService _summoners;
        private readonly IUserService _users;

        public LeagueUpdateJob(IUserService users, IRiotService riot, ISummonerService summoners)
        {
            _users = users;
            _riot = riot;
            _summoners = summoners;
        }

        public void Execute(int summonerId)
        {
            ExecuteInternal(summonerId).Wait();
        }

        private async Task ExecuteInternal(int summonerId)
        {
            var summoner = await _summoners.FindAsync(summonerId);
            if (summoner == null)
                return;

            var leagues = await _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);
            var solo = leagues?.FirstOrDefault(league => league.Queue == QueueType.RANKED_SOLO_5x5);

            if (solo == null)
            {
                await _summoners.UpdateLeagueAsync(summoner, TierName.Unranked, 0);
            }
            else
            {
                var entry = solo.Entries.First(e => e.PlayerOrTeamId == summoner.SummonerId.ToString());
                var division = (int) entry.Division;
                var tier = (TierName) Enum.Parse(typeof (TierName), solo.Tier.ToString(), true);
                await _summoners.UpdateLeagueAsync(summoner, tier, division);
            }
        }
    }
}