using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Hallam.RedditRankedFlairs.Areas.AdminPanel
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly Func<Claim, bool> ClaimEvaluator =
            claim =>
                claim != null && claim.Type == ClaimTypes.Role && (claim.Value == "admin" || claim.Value == "moderator");

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return base.AuthorizeCore(httpContext) && AuthorizeCoreInternal(httpContext);
        }

        private bool AuthorizeCoreInternal(HttpContextBase context)
        {
            var user = context.User;
            var identity = user.Identity as ClaimsIdentity;
            return identity != null && identity.IsAuthenticated && identity.Claims.Any(ClaimEvaluator);
        }
    }
}