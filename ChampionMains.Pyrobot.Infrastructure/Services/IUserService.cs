using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    /// <summary>
    ///     Manages the users of the application.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        ///     Adds a user to the repository asynchronously.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> AddAsync(User user);

        /// <summary>
        ///     Creates a new and adds it to the repository asynchronously.
        /// </summary>
        /// <param name="name">The user name.</param>
        /// <returns>A new user object.</returns>
        Task<User> CreateAsync(string name);

        /// <summary>
        ///     Creates an internal identity for a user using an external identity asynchronously.
        /// </summary>
        /// <param name="user">A user.</param>
        /// <param name="authenticationType">The OWIN authentication type.</param>
        /// <param name="externalIdentity">The external login identity.</param>
        /// <returns>A new claim identity object.</returns>
        Task<ClaimsIdentity> CreateIdentityAsync(User user, string authenticationType, ClaimsIdentity externalIdentity);

        /// <summary>
        ///     Finds a user by its name asynchronously.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <returns>A user object.</returns>
        Task<User> FindAsync(string userName);

        /// <summary>
        ///     Finds a user by its id asynchronously.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>A user object.</returns>
        Task<User> FindAsync(int userId);

            /// <summary>
        ///     Removes a user from the repository asynchronously.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> RemoveAsync(int userId);
    }
}