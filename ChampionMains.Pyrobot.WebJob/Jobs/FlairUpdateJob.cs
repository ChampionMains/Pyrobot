using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJob;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    public class FlairUpdateJob
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserService _userService;
        private readonly RedditService _redditService;
        private readonly FlairService _flairService;

        public FlairUpdateJob(UnitOfWork unitOfWork, UserService userService, RedditService redditService, FlairService flairService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _redditService = redditService;
            _flairService = flairService;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.FlairUpdate)] FlairUpdateMessage data)
        {
            var user = await _userService.FindAsync(data.UserId);

            var subreddit = _unitOfWork.Subreddits.FirstOrDefault(s => s.Id == data.SubredditId
                && (s.RankEnabled || s.ChampionMasteryEnabled) && (!s.AdminOnly || user.IsAdmin));

            if (subreddit == null)
                return;

            var existingFlair = await _redditService.GetFlairAsync(subreddit.Name, user.Name);

            var classes = _flairService.GenerateFlairCSS(user.Id, subreddit.ChampionId, subreddit.RankEnabled && data.RankEnabled,
                subreddit.ChampionMasteryEnabled && data.ChampionMasteryEnabled,
                subreddit.PrestigeEnabled && data.PrestigeEnabled, existingFlair?.CssClass);
            await _redditService.SetFlairAsync(subreddit.Name, user.Name, data.FlairText ?? existingFlair?.Text, classes);
        }
    }
}