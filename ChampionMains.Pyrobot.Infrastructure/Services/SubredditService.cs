using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class SubredditService
    {
        private readonly UnitOfWork _context;

        public SubredditService(UnitOfWork context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(string name)
        {
            _context.Subreddits.Add(new Subreddit {Name = name});
            return await _context.SaveChangesAsync() > 0;
        } 

        public async Task<ICollection<Subreddit>> GetAllAsync()
        {
            return await _context.Subreddits.OrderBy(sub => sub.Name).ToListAsync();
        }
    }
}