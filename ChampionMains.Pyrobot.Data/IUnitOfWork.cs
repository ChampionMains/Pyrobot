using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Data
{
    public interface IUnitOfWork
    {
        IDbSet<LeagueInfo> Leagues { get; }
        IDbSet<SubReddit> SubReddits { get; }
        IDbSet<Summoner> Summoners { get; }
        IDbSet<User> Users { get; }

        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}