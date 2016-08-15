using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Riot;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

namespace ChampionMains.Pyrobot.Services
{
    public class SummonerService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly TimeSpan _riotUpdateMaxStaleTime;

        public SummonerService(UnitOfWork unitOfWork, ApplicationConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _riotUpdateMaxStaleTime = config.RiotUpdateMax;
        }

        public async Task<IList<Summoner>> GetSummonersForUpdateAsync()
        {
            var staleAfter = DateTimeOffset.Now - _riotUpdateMaxStaleTime;
            return await _unitOfWork.Summoners.Where(s => s.LastUpdate == null || s.LastUpdate < staleAfter)
                .Include(s => s.User)
                .Include(s => s.Rank)
                .Include(s => s.ChampionMasteries).ToListAsync();
        }

        public Summoner AddSummoner(int userId, long summonerId, string region, string name, int profileIconId)
        {
            var summoner = _unitOfWork.Summoners.FirstOrDefault(s => s.UserId == userId && s.Region == region && s.SummonerId == summonerId);
            if (summoner == null)
            {
                summoner = new Summoner
                {
                    Rank = new SummonerRank(),
                    UserId = userId,
                    Region = region,
                    SummonerId = summonerId
                };
                _unitOfWork.Summoners.Add(summoner);
            }

            summoner.Name = name;
            summoner.ProfileIconId = profileIconId;
            return summoner;
        }

        public void UpdateSummoner(Summoner summoner, string region, string name, int profileIconId,
            Tier? tier, byte? division, ICollection<ChampionMastery> championMastery)
        {
            //_unitOfWork.Entry(summoner).Reference(s => s.Rank).Load();
            //_unitOfWork.Entry(summoner).Collection(s => s.ChampionMasteries).Load();

            summoner.Name = name;
            summoner.ProfileIconId = profileIconId;
            
            summoner.Rank.Division = division ?? 0;
            summoner.Rank.Tier = (byte?) tier ?? 0;

            foreach (var updated in championMastery)
            {
                var champMastery = summoner.ChampionMasteries.FirstOrDefault(x => x.ChampionId == updated.ChampionId);

                if (champMastery == null)
                {
                    //started playing a new champion
                    var champion = _unitOfWork.Champions.Find((short) updated.ChampionId);
                    if (champion == null)
                        continue; // possibly a new champion has been added, and the database needs updating

                    champMastery = new SummonerChampionMastery
                    {
                        Champion = champion
                    };

                    summoner.ChampionMasteries.Add(champMastery);
                }

                champMastery.Level = (byte) updated.ChampionLevel;
                champMastery.Points = updated.ChampionPoints;
            }

            summoner.LastUpdate = DateTimeOffset.Now;
        }

        public Summoner FindSummonerIncludeRankAndChampionMasteries(int id)
        {
            return _unitOfWork.Summoners
                .Include(s => s.Rank)
                .Include(s => s.ChampionMasteries)
                .FirstOrDefault(s => s.Id == id);
        }

        //public Task<Summoner> FindSummonerAsync(string region, string summonerName)
        //{
        //    return _unitOfWork.Summoners.FirstOrDefaultAsync(summoner =>
        //        summoner.Region == region &&
        //        summoner.Name == summonerName);
        //}

        public Task<bool> IsSummonerRegisteredAsync(string region, string summonerName)
        {
            return _unitOfWork.Summoners.AnyAsync(summoner =>
                summoner.Region == region &&
                summoner.Name == summonerName);
        }

        public async Task<bool> RemoveAsync(int summonerId)
        {
            var entity = await _unitOfWork.Summoners
                .Include(s => s.Rank)
                .FirstOrDefaultAsync(s => s.Id == summonerId);
            if (entity == null) return false;

            _unitOfWork.Leagues.Remove(entity.Rank);
            _unitOfWork.Summoners.Remove(entity);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public Task<int> SaveChangesAsync()
        {
            return _unitOfWork.SaveChangesAsync();
        }
    }
}