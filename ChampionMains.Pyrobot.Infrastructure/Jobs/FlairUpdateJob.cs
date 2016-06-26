using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.Jobs
{
    public class FlairUpdateJob
    {
        private readonly FlairService _flairs;
        private readonly UserService _users;
        private readonly SummonerService _summoners;
        private readonly RedditService _reddit;
        private readonly SubRedditService _subReddits;

        public FlairUpdateJob(FlairService flairs, UserService users, SummonerService summoners, RedditService reddit, SubRedditService subReddits)
        {
            _flairs = flairs;
            _users = users;
            _summoners = summoners;
            _reddit = reddit;
            _subReddits = subReddits;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.FlairUpdate)] int userId)
        {
            var user = await _users.FindAsync(userId);

            var subReddits = await _subReddits.GetAllAsync();

            var summoner = await _summoners.GetActiveSummonerAsync(user);
            var css = summoner == null ? "" : "rank-" + ((Tier)summoner.Rank.Tier);
            css = css.ToLower();

            await Task.WhenAll(subReddits.Select(x => _reddit.SetUserFlairAsync(x.Name, user.Name, "", css)));
        }
    }
}