using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class FlairService
    {
        private readonly UnitOfWork _context;

        public FlairService(UnitOfWork context)
        {
            _context = context;
        }

        public async Task<ICollection<User>> GetUsersForUpdateAsync(int max = 100)
        {
            var query = from user in _context.Users
                        where user.FlairUpdateRequiredTime.HasValue
                              || !user.FlairUpdatedTime.HasValue
                        orderby user.FlairUpdateRequiredTime
                        select user;

            return await query.Take(max).ToListAsync();
        }

        public async Task<bool> SetUpdateFlagAsync(IEnumerable<User> users, bool requiresUpdate = true)
        {
            foreach (var user in users)
            {
                user.FlairUpdateRequiredTime = requiresUpdate ? DateTimeOffset.Now : (DateTimeOffset?)null;
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SetUpdatedAsync(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                user.FlairUpdateRequiredTime = null;
                user.FlairUpdatedTime = DateTimeOffset.Now;
            }
            return await _context.SaveChangesAsync() > 0;
        }
    }
}