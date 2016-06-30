using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
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

        public async Task<bool> UpdateSubRedditUser(int userId, int subredditId, bool rankEnabled, bool championMasteryEnaabled, string flairText)
        {
            var subRedditUser =
                _context.SubRedditUsers.FirstOrDefault(u => u.UserId == userId && u.SubRedditId == subredditId);

            if (subRedditUser == null)
            {
                var subreddit = _context.SubReddits.Find(subredditId);
                if (subreddit == null)
                    return false;

                subRedditUser = new SubRedditUser()
                {
                    UserId = userId,
                    SubRedditId = subredditId
                };
                _context.SubRedditUsers.Add(subRedditUser);
            }
            
            subRedditUser.RankEnabled = rankEnabled;
            subRedditUser.ChampionMasteryEnabled = championMasteryEnaabled;
            subRedditUser.FlairText = flairText;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbEntityValidationException)
            {
                return false;
            }
        }
    }
}