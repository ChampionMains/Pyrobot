using System.Collections.Generic;
using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace Hallam.RedditRankedFlairs.Security
{
    public class HangfireDashboardAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            var owin = new OwinContext(owinEnvironment);
            var auth = owin.Authentication;
            var identity = auth.User.Identity as ClaimsIdentity;

            return identity != null && (identity.HasClaim(ClaimTypes.Role, "admin") || identity.HasClaim(ClaimTypes.Role, "moderator"));
        }
    }
}