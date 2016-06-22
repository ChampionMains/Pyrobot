using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class SubRedditService : ISubRedditService
    {
        private readonly IUnitOfWork _context;

        public SubRedditService(IUnitOfWork context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(string name)
        {
            _context.SubReddits.Add(new SubReddit {Name = name});
            return await _context.SaveChangesAsync() > 0;
        } 

        public async Task<ICollection<SubReddit>> GetAllAsync()
        {
            return await _context.SubReddits.OrderBy(sub => sub.Name).ToListAsync();
        }
    }
}