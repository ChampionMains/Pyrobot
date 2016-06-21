using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    public class SummonerService : ISummonerService
    {
        protected IUnitOfWork UnitOfWork;

        public SummonerService(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public async Task<Summoner> AddSummonerAsync(User user, int summonerId, string region, string name)
        {
            user.Summoners.Add(new Summoner
            {
                LeagueInfo = new LeagueInfo(),
                Name = name,
                Region = region,
                SummonerId = summonerId,
                User = user
            });
            await UnitOfWork.SaveChangesAsync();
            return user.Summoners.First(summoner => summoner.SummonerId == summonerId);
        }

        public Task<Summoner> FindAsync(int id)
        {
            return UnitOfWork.Summoners.FirstOrDefaultAsync(summoner =>
                summoner.Id == id);
        }

        public Task<Summoner> FindAsync(string region, string summonerName)
        {
            return UnitOfWork.Summoners.FirstOrDefaultAsync(summoner =>
                summoner.Region == region &&
                summoner.Name == summonerName);
        }

        public Task<Summoner> GetActiveSummonerAsync(User user)
        {
            return Task.Run(() => user.Summoners.FirstOrDefault(x => x.IsActive));
        } 

        public Task<bool> IsSummonerRegistered(string region, string summonerName)
        {
            return UnitOfWork.Summoners.AnyAsync(summoner =>
                summoner.Region == region &&
                summoner.Name == summonerName);
        }

        public async Task<bool> RemoveAsync(string region, string summonerName)
        {
            var entity = await FindAsync(region, summonerName);
            if (entity == null) return false;
            if (entity.IsActive)
            {
                // pass the active flag to another summoner if one exists.
                var summoner = entity.User.Summoners.FirstOrDefault(x => x.Id != entity.Id);
                if (summoner != null)
                {
                    summoner.IsActive = true;
                }
            }
            UnitOfWork.Leagues.Remove(entity.LeagueInfo);
            UnitOfWork.Summoners.Remove(entity);
            return await UnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> SetActiveSummonerAsync(Summoner summoner)
        {
            foreach (var s in summoner.User.Summoners)
            {
                s.IsActive = false;
            }
            summoner.IsActive = true;
            return await UnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLeagueAsync(Summoner summoner, TierName tier, int division)
        {
            summoner.LeagueInfo.Division = division;
            summoner.LeagueInfo.Tier = tier;
            summoner.LeagueInfo.UpdatedTime = DateTimeOffset.Now;
            return await UnitOfWork.SaveChangesAsync() > 0;
        } 
    }
}