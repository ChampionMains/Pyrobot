using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Services
{
    public interface IRedditMessengerService
    {
        /// <summary>
        /// Sends a private message over the Reddit messaging service.
        /// </summary>
        /// <param name="toUserName">The user name of the recipient.</param>
        /// <param name="subject">A subject string no longer than 100 characters.</param>
        /// <param name="message">The message body.</param>
        /// <returns>True if successful.</returns>
        Task<bool> SendMessageAsync(string toUserName, string subject, string message);
    }
}