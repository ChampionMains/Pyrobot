using System.Collections.Generic;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
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