using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using MingweiSamuel.Camille.ChampionMasteryV4;
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
            const int maxBatches = 2;
            const int batchSize = 100;

            var staleAfter = DateTimeOffset.Now - _riotUpdateMaxStaleTime;
            var result = new List<Summoner>();

            var i = 0;
            while(true)
            {
                var data = await _unitOfWork.Summoners
                    .Where(s => s.LastUpdate == null || s.LastUpdate < staleAfter)
                    .OrderBy(s => s.LastUpdate)
                    .ThenBy(s => s.Id)
                    .Skip(batchSize * i).Take(batchSize)
                    .OrderBy(s => s.Region)
                    .Include(s => s.User)
                    .Include(s => s.Rank)
                    .Include(s => s.ChampionMasteries)
                    .ToListAsync();

                if (data.Count <= 0)
                    break;

                result.AddRange(data);
                i++;

                if (i >= maxBatches)
                    break;
            }

            return result;
        }

        public IList<Summoner> GetSummonersIncludeDataByUserId(int userId)
        {
            return _unitOfWork.Summoners.Where(s => s.UserId == userId)
                .Include(s => s.Rank)
                .Include(s => s.ChampionMasteries).ToList();
        }

        public Summoner AddSummoner(int userId, string summonerIdEnc, string region, string name, int profileIconId)
        {
            var summoner = _unitOfWork.Summoners.FirstOrDefault(s => s.UserId == userId && s.Region == region && s.SummonerIdEnc == summonerIdEnc);
            if (summoner == null)
            {
                summoner = new Summoner
                {
                    Rank = new SummonerRank(),
                    UserId = userId,
                    Region = region,
                    SummonerId = null,
                    SummonerIdEnc = summonerIdEnc
                };
                _unitOfWork.Summoners.Add(summoner);
            }

            summoner.Name = name;
            summoner.ProfileIconId = profileIconId;
            return summoner;
        }

        private HashSet<short> _validChampions;
        private DateTimeOffset _lastUpdate = DateTimeOffset.MinValue;
        private HashSet<short> ValidChampions
        {
            get
            {
                if (_validChampions == null || DateTimeOffset.Now > _lastUpdate + TimeSpan.FromMinutes(1))
                {
                    _validChampions = new HashSet<short>(_unitOfWork.Champions.Select(c => c.Id));
                    _lastUpdate = DateTimeOffset.Now;
                }
                return _validChampions;
            }
        }

        public void UpdateSummoner(Summoner summoner, string region, string name, int profileIconId,
            Tier? tier, byte? division, ICollection<ChampionMastery> championMastery)
        {
            summoner.Name = name;
            summoner.ProfileIconId = profileIconId;

            summoner.Rank.Division = division ?? 0;
            summoner.Rank.Tier = (byte?) tier ?? 0;

            foreach (var updated in championMastery)
            {
                var champMastery = summoner.ChampionMasteries.FirstOrDefault(x => x.ChampionId == updated.ChampionId);

                if (champMastery == null)
                {
                    // started playing a new champion
                    if (!ValidChampions.Contains((short) updated.ChampionId))
                        continue; // possibly a new champion has been added, and the database needs updating

                    champMastery = new SummonerChampionMastery
                    {
                        ChampionId = (short) updated.ChampionId
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

        public Task<Summoner> FindSummonerAsync(string region, string summonerIdEnc)
        {
            return _unitOfWork.Summoners.FirstOrDefaultAsync(summoner =>
                summoner.Region == region &&
                summoner.SummonerIdEnc == summonerIdEnc);
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

        public int SaveChanges()
        {
            return _unitOfWork.SaveChanges();
        }
    }
}