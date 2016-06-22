using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Data
{
    public interface IUnitOfWork
    {
        IDbSet<SummonerInfo> Leagues { get; }
        IDbSet<SubReddit> SubReddits { get; }
        IDbSet<Summoner> Summoners { get; }
        IDbSet<User> Users { get; }

        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}