using System;
using System.Collections.Specialized;
using System.Linq;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using RiotSharp;
using SimpleInjector;

namespace ChampionMains.Pyrobot.Startup
{
    public static class SharedSimpleInjectorConfig
    {
        public static void Configure(Container container, NameValueCollection s)
        {
            // Config
            container.Register(() => new ApplicationConfiguration
            {
                FlairBotVersion = s["bot.version"],
                RiotUpdateMin = TimeSpan.Parse(s["website.riotUpdateMin"]),
                RiotUpdateMax = TimeSpan.Parse(s["website.riotUpdateMax"]),
                FlairUpdate = TimeSpan.Parse(s["website.flairUpdate"] ?? s["website.flairUpdateMax"]),
            }, Lifestyle.Singleton);

            // Services
            container.Register<UserService>(Lifestyle.Scoped);
            container.Register<SummonerService>(Lifestyle.Scoped);
            container.Register<FlairService>(Lifestyle.Scoped);
            // above use db, below do not
            container.Register<RedditService>(Lifestyle.Singleton);
            container.Register<ValidationService>(Lifestyle.Singleton);
            container.Register(() => new RoleService(
                s["security.admins"].Split(',').Select(x => x.Trim()).ToList()), Lifestyle.Singleton);
            container.Register(() => RiotApi.GetInstance(s["riot.apiKey"],
                int.Parse(s["riot.rateLimit10s"]), int.Parse(s["riot.rateLimit10m"])));
            //container.Register(() => new RiotService
            //{
            //    WebRequester = new RiotWebRequester(s["riot.rateLimit"])
            //    {
            //        ApiKey = s["riot.apiKey"],
            //        MaxAttempts = int.Parse(s["riot.maxAttempts"]),
            //        RetryInterval = TimeSpan.Parse(s["riot.retryInterval"])
            //    }
            //}, Lifestyle.Singleton);

            // Reddit WebRequester
            container.Register(() => new RedditWebRequester(
                    s["reddit.script.clientId"],
                    s["reddit.script.clientSecret"],
                    s["reddit.modUserName"],
                    s["reddit.modPassword"],
                    s["userAgent"]), Lifestyle.Singleton);

            // Database
            container.Register<UnitOfWork>(Lifestyle.Scoped);

            // verify container (creates test instance of each)
            container.Verify();
        }
    }
}
