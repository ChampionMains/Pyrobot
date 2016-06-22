using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin;

namespace ChampionMains.Pyrobot.Security
{
    //public class Hang-fireDashboardAuthorizationFilter : IAuthorizationFilter
    //{
    //    public bool Authorize(IDictionary<string, object> owinEnvironment)
    //    {
    //        var owin = new OwinContext(owinEnvironment);
    //        var auth = owin.Authentication;
    //        var identity = auth.User.Identity as ClaimsIdentity;

    //        return identity != null && (identity.HasClaim(ClaimTypes.Role, "admin") || identity.HasClaim(ClaimTypes.Role, "moderator"));
    //    }
    //}
}