using System;
using System.Data.Entity;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Data
{
    public class UnitOfWork : DbContext
    {
        public IDbSet<SummonerRank> Leagues { get; set; }
        public IDbSet<SubReddit> SubReddits { get; set; }
        public IDbSet<SubRedditUser> SubRedditUsers { get; set; }
        public IDbSet<Summoner> Summoners { get; set; }
        public IDbSet<User> Users { get; set; }
        public IDbSet<Champion> Champions { get; set; }

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