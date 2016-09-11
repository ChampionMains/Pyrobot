using System.Web.Mvc;
using System.Web.Routing;

namespace ChampionMains.Pyrobot
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();
            //routes.MapRoute("Profile_Summoner", "profile/summoner/{region}/{summonerId}/{action}",
            //    new {controller = "ProfileSummoner"});
            //routes.MapRoute("Profile", "profile/{action}", new { controller = "Profile", action = "index" });
            //routes.MapRoute("Login", "{action}", new { controller = "Login", action = "index" });
        }
    }
}
