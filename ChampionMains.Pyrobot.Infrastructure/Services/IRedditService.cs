using System.Collections.Generic;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Reddit;
using Hallam.RedditRankedFlairs.Services.Reddit;

namespace Hallam.RedditRankedFlairs.Services
{
    /// <summary>
    ///     Manages requests to the Reddit API.
    /// </summary>
    public interface IRedditService
    {
        Task<ICollection<string>> GetSubRedditsAsync(SubRedditKind kind);

        /// <summary>
        ///     Sets the user flair for a user on a subreddit.
        /// </summary>
        /// <param name="subreddit">The subreddit.</param>
        /// <param name="name">The name of the user.</param>
        /// <param name="text">The flair text.</param>
        /// <param name="css">The CSS class.</param>
        /// <returns>True indicates success.</returns>
        Task<bool> SetUserFlairAsync(string subreddit, string name, string text, string css = null);

        /// <summary>
        ///     Sets flairs for up to one hundred users in a bulk request.
        /// </summary>
        /// <param name="subreddit">The subreddit.</param>
        /// <param name="flairs">A collection of flair parameters.</param>
        /// <returns>True indicates success</returns>
        Task<bool> SetUserFlairsAsync(string subreddit, ICollection<UserFlairParameter> flairs);
    }
}