using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
