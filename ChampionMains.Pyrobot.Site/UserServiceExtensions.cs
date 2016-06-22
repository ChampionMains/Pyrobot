using System.Threading.Tasks;
using System.Web;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot
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