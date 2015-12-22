﻿using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    /// <summary>
    ///     Manages the summoners which are registered to users.
    /// </summary>
    public interface ISummonerService
    {
        /// <summary>
        ///     Adds a summoner to a user asynchronously.
        /// </summary>
        /// <param name="user">The user who owns the summoner.</param>
        /// <param name="summonerId">The summoner ID.</param>
        /// <param name="region">The region.</param>
        /// <param name="name">The summoner name.</param>
        /// <returns>A new summoner object.</returns>
        Task<Summoner> AddSummonerAsync(User user, int summonerId, string region, string name);

            /// <summary>
        ///     Finds a summoner asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerName">The summoner name.</param>
        /// <returns>A summoner object.</returns>
        Task<Summoner> FindAsync(string region, string summonerName);

        /// <summary>
        ///     Determines if a summoner is registered asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerName">The summoner name.</param>
        /// <returns>A value indicating whether the summoner is registered.</returns>
        Task<bool> IsSummonerRegistered(string region, string summonerName);

        /// <summary>
        ///     Removes a summoner from the repository asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerName">The summoner name.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> RemoveAsync(string region, string summonerName);

        /// <summary>
        ///     Assigns the summoner as its user's active summoner asynchronously.
        /// </summary>
        /// <param name="summoner">The summoner to assign.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> SetActiveSummonerAsync(Summoner summoner);

        /// <summary>
        ///     Updates the league standing for a summoner.
        /// </summary>
        /// <param name="summoner">The summoner.</param>
        /// <param name="tier">The league tier.</param>
        /// <param name="division">The league division.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> UpdateLeagueAsync(Summoner summoner, TierName tier, int division);
    }
}