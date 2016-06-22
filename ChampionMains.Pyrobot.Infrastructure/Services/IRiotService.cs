using System.Collections.Generic;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Riot;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

namespace ChampionMains.Pyrobot.Services
{
    /// <summary>
    ///     Conducts requests to the Riot League of Legends Web API.
    /// </summary>
    public interface IRiotService
    {
        /// <summary>
        ///     Finds a summoner asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerName">The summoner name.</param>
        /// <returns>A summoner object.</returns>
        Task<Summoner> FindSummonerAsync(string region, string summonerName);

        /// <summary>
        ///     Retrieves a collection of a summoner's leagues asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerId">The summoner id.</param>
        /// <returns>A collection of league objects.</returns>
        Task<ICollection<League>> GetLeaguesAsync(string region, long summonerId);

        /// <summary>
        ///     Retrieves a collection of a summoner's rune pages asynchronously.
        /// </summary>
        /// <param name="region">The summoner region.</param>
        /// <param name="summonerId">The summoner id.</param>
        /// <returns>A collection of rune page objects.</returns>
        Task<ICollection<RunePage>> GetRunePagesAsync(string region, long summonerId);
    }
}