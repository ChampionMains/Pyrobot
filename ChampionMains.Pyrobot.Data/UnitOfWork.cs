using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Data
{
    public class UnitOfWork : DbContext, IUnitOfWork
    {
        public IDbSet<LeagueInfo> Leagues { get; set; }
        public IDbSet<SubReddit> SubReddits { get; set; }
        public IDbSet<Summoner> Summoners { get; set; }
        public IDbSet<User> Users { get; set; }

        public UnitOfWork() { }
        public UnitOfWork(string nameOrConnectionString) : base(nameOrConnectionString) { }

        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}