using System.Web.Http;
using Hallam.RedditRankedFlairs.WebAPI;

namespace Hallam.RedditRankedFlairs
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new ApiMessageHandler());
            //config.Filters.Add(new RejectEmptyModelFilter());
        }
    }
}