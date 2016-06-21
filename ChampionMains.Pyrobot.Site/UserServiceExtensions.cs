using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Hallam.RedditRankedFlairs.Data;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs
{
    public static class UserServiceExtensions
    {
        public static Task<User> GetUserAsync(this IUserService service)
        {
            var identity = HttpContext.Current.User?.Identity;
            return identity == null || !identity.IsAuthenticated ? null : service.FindAsync(identity.Name);
        }
    }
}