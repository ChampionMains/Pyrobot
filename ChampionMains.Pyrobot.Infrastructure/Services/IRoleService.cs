using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Services
{
    /// <summary>
    ///     Manages the roles of users.
    /// </summary>
    public interface IRoleService
    {
        Task<bool> IsAdminAsync(string name);
    }
}