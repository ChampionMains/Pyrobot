using System.Collections.Generic;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    public interface ILeagueUpdateService
    {
        Task<ICollection<Summoner>> GetSummonersForUpdateAsync(int max);
        Task<bool> SetUpdatedAsync(IEnumerable<Summoner> summoners);
    }
}