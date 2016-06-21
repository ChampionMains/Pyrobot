using System.Collections.Generic;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    public interface IFlairService
    {
        /// <summary>
        ///     Gets a collection of users that require flair updates, asynchronously.
        /// </summary>
        /// <param name="max">The maximum number of users to retrieve.</param>
        /// <returns>A collection of User objects.</returns>
        Task<ICollection<User>> GetUsersForUpdateAsync(int max);

        Task<bool> SetUpdatedAsync(IEnumerable<User> users);

            /// <summary>
        ///     Sets a flag indicating whether a user requires flair updates, asynchronously.
        /// </summary>
        /// <param name="users">The users to update.</param>
        /// <param name="requiresUpdate">True indicates that a flair update is required.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> SetUpdateFlagAsync(IEnumerable<User> users, bool requiresUpdate = true);
    }
}