using System.Collections.Generic;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Services
{
    public interface ILeagueUpdateService
    {
        Task<ICollection<Summoner>> GetSummonersForUpdateAsync(int max);
        Task<bool> SetUpdatedAsync(IEnumerable<Summoner> summoners);
    }
}