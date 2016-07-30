﻿using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJob;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.Jobs
{
    public class FlairUpdateJob
    {
        private readonly UserService _users;
        private readonly RedditService _reddit;
        private readonly SubredditService _subreddits;

        public FlairUpdateJob(UserService users, RedditService reddit, SubredditService subreddits)
        {
            _users = users;
            _reddit = reddit;
            _subreddits = subreddits;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.FlairUpdate)] FlairUpdateMessage data)
        {
            var user = await _users.FindAsync(data.UserId);

            var subreddit = (await _subreddits.GetAllAsync()).FirstOrDefault(s => s.Id == data.SubredditId
                &&(s.RankEnabled || s.ChampionMasteryEnabled) && (!s.AdminOnly || user.IsAdmin));

            if (subreddit == null)
                return;

            var existingFlair = await _reddit.GetFlairAsync(subreddit.Name, user.Name);
            var classes = RankUtil.GenerateFlairCss(user, subreddit.ChampionId, subreddit.RankEnabled && data.RankEnabled,
                subreddit.ChampionMasteryEnabled && data.ChampionMasteryEnabled,
                subreddit.PrestigeEnabled && data.PrestigeEnabled, existingFlair?.CssClass);
            await _reddit.SetFlairAsync(subreddit.Name, user.Name, data.FlairText ?? existingFlair?.Text, classes);
        }
    }
}