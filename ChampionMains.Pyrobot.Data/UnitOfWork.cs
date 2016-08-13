using System;
using System.Data.Entity;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Data
{
    public class UnitOfWork : DbContext
    {
        public IDbSet<SummonerRank> Leagues { get; set; }
        public IDbSet<Subreddit> Subreddits { get; set; }
        public IDbSet<SubredditUserFlair> SubredditUserFlairs { get; set; }
        public IDbSet<Summoner> Summoners { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Champion> Champions { get; set; }

        public UnitOfWork()
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public UnitOfWork(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }
    }
}