using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
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
            _context.SubReddits.Add(new Subreddit {Name = name});
            return await _context.SaveChangesAsync() > 0;
        } 

        public async Task<ICollection<Subreddit>> GetAllAsync()
        {
            return await _context.SubReddits.OrderBy(sub => sub.Name).ToListAsync();
        }

        public async Task<ICollection<SubredditUserFlair>> GetSubRedditUserFlairs(User user)
        {
            return await _context.SubRedditUsers.Where(f => f.User.Id == user.Id).ToListAsync();
        }

        public async Task<bool> UpdateSubRedditUser(int userId, int subredditId, bool rankEnabled, bool championMasteryEnaabled, bool prestigeEnabled, string flairText)
        {
            var subredditUserFlair =
                _context.SubRedditUsers.FirstOrDefault(u => u.UserId == userId && u.SubredditId == subredditId);

            if (subredditUserFlair == null)
            {
                var subreddit = _context.SubReddits.Find(subredditId);
                if (subreddit == null)
                    return false;

                subredditUserFlair = new SubredditUserFlair()
                {
                    UserId = userId,
                    SubredditId = subredditId
                };
                _context.SubRedditUsers.Add(subredditUserFlair);
            }
            
            subredditUserFlair.RankEnabled = rankEnabled;
            subredditUserFlair.ChampionMasteryEnabled = championMasteryEnaabled;
            subredditUserFlair.PrestigeEnabled = prestigeEnabled;
            subredditUserFlair.FlairText = flairText;

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