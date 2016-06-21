using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Services
{
    /// <summary>
    ///     Manages the roles of users.
    /// </summary>
    public interface IRoleService
    {
        Task<bool> IsAdminAsync(string name);
    }
}