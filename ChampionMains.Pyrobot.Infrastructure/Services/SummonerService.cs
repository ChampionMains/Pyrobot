using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Services
{
    public class SummonerService
    {
        protected UnitOfWork UnitOfWork;

        public SummonerService(UnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public async Task<Summoner> AddSummonerAsync(User user, long summonerId, string region, string name)
        {
            var summoner = new Summoner
            {
                SummonerInfo = new SummonerInfo(),
                Name = name,
                Region = region,
                SummonerId = summonerId,
                User = user
            };
            user.Summoners.Add(summoner);
            await UnitOfWork.SaveChangesAsync();
            return summoner;
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
            //TODO
            return Task.Run(() => user.Summoners.FirstOrDefault());
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

            //TODO
            /*
            if (entity.IsActive)
            {
                // pass the active flag to another summoner if one exists.
                var summoner = entity.User.Summoners.FirstOrDefault(x => x.Id != entity.Id);
                if (summoner != null)
                {
                    summoner.IsActive = true;
                }
            }*/
            UnitOfWork.Leagues.Remove(entity.SummonerInfo);
            UnitOfWork.Summoners.Remove(entity);
            return await UnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> SetActiveSummonerAsync(Summoner summoner)
        {
            //foreach (var s in summoner.User.Summoners)
            //{
            //    s.IsActive = false;
            //}
            //summoner.IsActive = true;
            return await UnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateLeagueAsync(Summoner summoner, byte tier, byte division)
        {
            summoner.SummonerInfo.Division = division;
            summoner.SummonerInfo.Tier = tier;
            summoner.SummonerInfo.UpdatedTime = DateTimeOffset.Now;
            return await UnitOfWork.SaveChangesAsync() > 0;
        } 
    }
}