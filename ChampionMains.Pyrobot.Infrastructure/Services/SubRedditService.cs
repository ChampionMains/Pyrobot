using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class SubRedditService
    {
        private readonly UnitOfWork _context;

        public SubRedditService(UnitOfWork context)
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

        public async Task<ICollection<SubRedditUser>> GetSubRedditUsers(User user)
        {
            return await _context.SubRedditUsers.Where(f => f.User.Id == user.Id).ToListAsync();
        }

        public async Task<bool> UpdateSubRedditUser(User user, string subRedditName, bool rankEnabled, bool championMasteryEnaabled, string flairText)
        {
            var subRedditUser =
                _context.SubRedditUsers.FirstOrDefault(u => u.UserId == user.Id && u.SubReddit.Name == subRedditName);

            if (subRedditUser == null)
            {
                var subReddit = _context.SubReddits.FirstOrDefault(r => r.Name == subRedditName);
                if (subReddit == null)
                    return false;

                subRedditUser = new SubRedditUser()
                {
                    User = user,
                    SubReddit = subReddit
                };
                _context.SubRedditUsers.Add(subRedditUser);
            }
            
            subRedditUser.RankEnabled = rankEnabled;
            subRedditUser.ChampionMasteryEnabled = championMasteryEnaabled;
            subRedditUser.FlairText = flairText;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}