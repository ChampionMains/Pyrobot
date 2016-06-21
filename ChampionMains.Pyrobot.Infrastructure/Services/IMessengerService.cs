using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Services
{
    public interface IMessengerService
    {
        /// <summary>
        /// Sends a private message to a Reddit user.
        /// </summary>
        /// <param name="toUserName">The name of an existing user.</param>
        /// <param name="subject">A subject string no longer than 100 characters.</param>
        /// <param name="text">The message body in markdown syntax.</param>
        /// <returns>True if successful.</returns>
        Task<bool> SendMessageAsync(string toUserName, string subject, string text);
    }
}