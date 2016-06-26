using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class LeagueUpdateService
    {
        private readonly UnitOfWork _context;
        private readonly ApplicationConfiguration _config;

        public LeagueUpdateService(UnitOfWork context, ApplicationConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<ICollection<Summoner>> GetSummonersForUpdateAsync(int max)
        {
            var cutoff = DateTimeOffset.Now - _config.LeagueDataStaleTime;
            var query = from summoner in _context.Summoners
                        where summoner.Rank.UpdatedTime.HasValue
                              && summoner.Rank.UpdatedTime < cutoff
                        select summoner;
            return await query.Take(max).ToListAsync();
        }

        public async Task<bool> SetUpdatedAsync(IEnumerable<Summoner> summoners)
        {
            foreach (var s in summoners)
            {
                s.Rank.UpdatedTime = DateTimeOffset.Now;
            }
            return await _context.SaveChangesAsync() > 0;
        } 
    }
}