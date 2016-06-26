using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ChampionMains.Pyrobot.Attributes
{
    public class WebApiAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext ctx)
        {
            if (!ctx.RequestContext.Principal.Identity.IsAuthenticated)
                base.HandleUnauthorizedRequest(ctx);
            else
            {
                // Authenticated, but not AUTHORIZED.  Return 403 instead!
                ctx.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }
        }
    }
}