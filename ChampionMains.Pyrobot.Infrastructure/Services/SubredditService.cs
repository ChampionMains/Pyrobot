using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public class SubredditService
    {
        protected readonly UnitOfWork UnitOfWork;

        public SubredditService(UnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public async Task<IList<Subreddit>> GetSubreddits()
        {
            return await UnitOfWork.Subreddits.ToListAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return UnitOfWork.SaveChangesAsync();
        }
    }
}
