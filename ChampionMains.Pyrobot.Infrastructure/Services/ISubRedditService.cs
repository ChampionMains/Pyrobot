using System.Collections.Generic;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    /// <summary>
    ///     Manages SubReddit subscriptions.
    /// </summary>
    public interface ISubRedditService
    {
        Task<bool> AddAsync(string name);

        /// <summary>
        ///     Gets a collection of all subscribed sub reddits asynchronously.
        /// </summary>
        /// <returns>A collection of SubReddit objects.</returns>
        Task<ICollection<SubReddit>> GetAllAsync();
    }
}