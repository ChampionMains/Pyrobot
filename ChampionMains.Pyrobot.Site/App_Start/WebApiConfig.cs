using System.Web.Http;
using ChampionMains.Pyrobot.WebAPI;

namespace ChampionMains.Pyrobot
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