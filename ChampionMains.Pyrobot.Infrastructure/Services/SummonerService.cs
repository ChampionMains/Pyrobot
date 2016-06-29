using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

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
                Rank = new SummonerRank(),
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

        public Task<bool> IsSummonerRegistered(string region, string summonerName)
        {
            return UnitOfWork.Summoners.AnyAsync(summoner =>
                summoner.Region == region &&
                summoner.Name == summonerName);
        }

        public async Task<bool> RemoveAsync(int summonerId)
        {
            var entity = await FindAsync(summonerId);
            if (entity == null) return false;

            UnitOfWork.Leagues.Remove(entity.Rank);
            UnitOfWork.Summoners.Remove(entity);
            return await UnitOfWork.SaveChangesAsync() > 0;
        }

        public void UpdateLeagueAsync(Summoner summoner, byte tier, byte division)
        {
            summoner.Rank.Division = division;
            summoner.Rank.Tier = tier;
            summoner.Rank.UpdatedTime = DateTimeOffset.Now;
        }

        public void UpdateChampionMasteriesAsync(ICollection<ChampionMastery> championMasteries, Summoner summoner)
        {
            foreach (var updated in championMasteries)
            {
                var champMastery = summoner.ChampionMasteries.FirstOrDefault(x => x.ChampionId == updated.ChampionId);

                if (champMastery == null)
                {
                    //started playing a new champion
                    var champion = UnitOfWork.Champions.Find((short)updated.ChampionId);
                    if (champion == null)
                        continue; // possibly a new champion has been added, and the database needs updating
                    
                    champMastery = new SummonerChampionMastery()
                    {
                        Champion = champion
                    };

                    summoner.ChampionMasteries.Add(champMastery);
                }

                champMastery.Level = (byte) updated.ChampionLevel;
                champMastery.Points = updated.ChampionPoints;
                champMastery.UpdatedTime = DateTimeOffset.Now;
            }
        }

        public async Task SaveChanges()
        {
            await UnitOfWork.SaveChangesAsync();
        }
    }
}