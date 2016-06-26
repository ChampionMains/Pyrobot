using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

namespace ChampionMains.Pyrobot.Jobs
{
    public class BulkSummonerUpdateJob
    {
        private const int MaxEntriesPerLoop = 25;
        private static readonly TimeSpan NoUpdateWaitInterval = TimeSpan.FromMinutes(5);
        private static readonly Mutex Lock = new Mutex();

        private readonly RiotService _riot;
        private readonly UserService _users;
        private readonly LeagueUpdateService _leagues;
        private readonly SummonerService _summoners;
        private readonly FlairService _flair;

        public BulkSummonerUpdateJob(RiotService riot, UserService users, LeagueUpdateService leagues, SummonerService summoners, FlairService flair)
        {
            _riot = riot;
            _users = users;
            _leagues = leagues;
            _summoners = summoners;
            _flair = flair;
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
            catch (AggregateException e)
            {
                ExceptionDispatchInfo.Capture(e.Flatten().InnerExceptions.First()).Throw();
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
                var summoners = await _leagues.GetSummonersForUpdateAsync(MaxEntriesPerLoop);

                if (!summoners.Any())
                {
                    await Task.Delay(NoUpdateWaitInterval);
                    continue;
                }

                var tasks = summoners.Select(UpdateSummonerAsync);
                Task.WaitAll(tasks.ToArray());
                await _leagues.SetUpdatedAsync(summoners);
                await _flair.SetUpdateFlagAsync(summoners.Select(x => x.User));
                await Task.Delay(1000);
            }
        }

        private async Task UpdateSummonerAsync(Summoner summoner)
        {
            if (summoner == null)
                throw new ArgumentNullException(nameof(summoner));

            if (summoner.Rank == null)
                throw new InvalidOperationException("summoner.LeagueInfo is null");

            var rank = await _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);

            summoner.Rank.Tier = (byte) rank.Item1;
            summoner.Rank.Division = rank.Item2;
            summoner.Rank.UpdatedTime = DateTimeOffset.Now;
        }
    }
}