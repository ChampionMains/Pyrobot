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

            //TODO: optimize
            var subReddits = (await _subReddits.GetAllAsync()).Where(s => (s.RankEnabled || s.ChampionMasteryEnabled) && (!s.AdminOnly || user.IsAdmin));

            await Task.WhenAll(subReddits.Select(async x =>
            {
                var oldFlair = await _reddit.GetFlairAsync(x.Name, user.Name);
                var classes = RankUtil.GenerateFlairCss(user, x.ChampionId, x.RankEnabled, x.ChampionMasteryEnabled, oldFlair?.CssClass);
                await _reddit.SetFlairAsync(x.Name, user.Name, oldFlair.Text, classes);
            }));
        }
    }
}