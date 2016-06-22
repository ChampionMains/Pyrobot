using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Jobs
{
    public class BulkFlairUpdateJob
    {
        private const int MaxUsersPerLoop = 100;
        private static readonly TimeSpan NoUsersWaitInterval = TimeSpan.FromSeconds(30);
        private static readonly Mutex Lock = new Mutex();

        private readonly IFlairService _flairs;
        private readonly IUserService _users;
        private readonly IRedditService _reddit;
        private readonly ISubRedditService _subReddits;
        private readonly ISummonerService _summoners;

        public BulkFlairUpdateJob(IUserService users, IFlairService flairs, IRedditService reddit, ISubRedditService subReddits, ISummonerService summoners)
        {
            _flairs = flairs;
            _users = users;
            _reddit = reddit;
            _subReddits = subReddits;
            _summoners = summoners;
        }

        public void Execute()
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
                var users = await _flairs.GetUsersForUpdateAsync(MaxUsersPerLoop);
                var subReddits = await _subReddits.GetAllAsync();

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

                foreach (var sub in subReddits)
                {
                    if (!await _reddit.SetUserFlairsAsync(sub.Name, flairParams))
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

        private async Task<string> GetFlairTextAsync(User user)
        {
            var summoner = await _summoners.GetActiveSummonerAsync(user);
            return summoner == null ? "" : LeagueUtil.Stringify(summoner.SummonerInfo);
        }
    }
}