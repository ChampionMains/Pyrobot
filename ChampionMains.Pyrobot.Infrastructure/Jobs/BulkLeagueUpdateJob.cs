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
    public class BulkLeagueUpdateJob
    {
        private const int MaxEntriesPerLoop = 25;
        private static readonly TimeSpan NoUpdateWaitInterval = TimeSpan.FromMinutes(5);
        private static readonly Mutex Lock = new Mutex();

        private readonly IRiotService _riot;
        private readonly IUserService _users;
        private readonly ILeagueUpdateService _leagues;
        private readonly ISummonerService _summoners;
        private readonly IFlairService _flair;

        public BulkLeagueUpdateJob(IRiotService riot, IUserService users, ILeagueUpdateService leagues, ISummonerService summoners, IFlairService flair)
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
                throw new ArgumentNullException("summoner");

            if (summoner.SummonerInfo == null)
                throw new InvalidOperationException("summoner.LeagueInfo is null");

            var leagues = await _riot.GetLeaguesAsync(summoner.Region, summoner.SummonerId);
            var solo = leagues?.FirstOrDefault(league => league.Queue == QueueType.RANKED_SOLO_5x5);

            if (solo == null)
            {
                summoner.SummonerInfo.Division = 0;
                summoner.SummonerInfo.Tier = (byte) Tiers.Unranked;
                summoner.SummonerInfo.UpdatedTime = DateTimeOffset.Now;
            }
            else
            {
                var entry = solo.Entries.First(e => e.PlayerOrTeamId == summoner.SummonerId.ToString());

                if (entry == null)
                    throw new InvalidOperationException("Entry not found in league.");

                var division = (byte)entry.Division;
                var tier = (Tiers)Enum.Parse(typeof(Tiers), solo.Tier.ToString(), true);
                summoner.SummonerInfo.Division = division;
                summoner.SummonerInfo.Tier = (byte) tier;
                summoner.SummonerInfo.UpdatedTime = DateTimeOffset.Now;
            }
        }
    }
}