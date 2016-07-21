using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.Jobs
{
    public class BulkFlairUpdateJob
    {
        private static readonly TimeSpan NoUsersWaitInterval = TimeSpan.FromSeconds(30);
        private static readonly Mutex Lock = new Mutex();
        private readonly FlairService _flairs;
        private readonly UserService _users;
        private readonly RedditService _reddit;
        private readonly SubredditService _subreddits;
        private readonly SummonerService _summoners;

        public BulkFlairUpdateJob(UserService users, FlairService flairs, RedditService reddit, SubredditService subreddits, SummonerService summoners)
        {
            _flairs = flairs;
            _users = users;
            _reddit = reddit;
            _subreddits = subreddits;
            _summoners = summoners;
        }

        public void Execute([QueueTrigger(WebJobQueue.BulkUpdate)] string data)
        {
            if (!Lock.WaitOne(1000))
            {
                return;
            }
            try
            {
                ExecuteInternal().Wait();
            }
            finally
            {
                Lock.ReleaseMutex();
            }
        }

        private async Task ExecuteInternal()
        {
            while (true)
            {
                var users = await _flairs.GetUsersForUpdateAsync();
                var subreddits = await _subreddits.GetAllAsync();

                if (!users.Any())
                {
                    await Task.Delay(NoUsersWaitInterval);
                    continue;
                }

                var flairParams = new List<UserFlairParameter>();

                foreach (var user in users)
                {
                    flairParams.Add(new UserFlairParameter
                    {
                        Name = user.Name,
                        Text = await GetFlairTextAsync(user)
                    });
                }

                foreach (var sub in subreddits)
                {
                    if (!await _reddit.SetFlairsAsync(sub.Name, flairParams))
                    {
                        throw new InvalidOperationException($"Update flair failed on /r/{sub.Name}.");
                    }
                }

                if (!await _flairs.SetUpdatedAsync(users))
                {
                    throw new InvalidOperationException("Unable to clear flair update flag.");
                }

                await Task.Delay(1);
            }
        }

        //TODO
        private Task<string> GetFlairTextAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}