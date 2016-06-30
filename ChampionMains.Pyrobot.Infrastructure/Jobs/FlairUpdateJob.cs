using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJob;
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

        public async Task Execute([QueueTrigger(WebJobQueue.FlairUpdate)] FlairUpdateMessage data)
        {
            var user = await _users.FindAsync(data.UserId);

            var subReddit = (await _subReddits.GetAllAsync()).FirstOrDefault(s => s.Id == data.SubRedditId
                &&(s.RankEnabled || s.ChampionMasteryEnabled) && (!s.AdminOnly || user.IsAdmin));

            if (subReddit == null)
                return;

            var oldFlair = await _reddit.GetFlairAsync(subReddit.Name, user.Name);
            var classes = RankUtil.GenerateFlairCss(user, subReddit.ChampionId, subReddit.RankEnabled & data.RankEnabled,
                subReddit.ChampionMasteryEnabled & data.ChampionMasteryEnabled, oldFlair?.CssClass);
            await _reddit.SetFlairAsync(subReddit.Name, user.Name, data.FlairText ?? oldFlair?.Text, classes);
        }
    }
}