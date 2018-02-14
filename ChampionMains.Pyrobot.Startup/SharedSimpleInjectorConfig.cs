using System;
using System.Collections.Specialized;
using System.Linq;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using MingweiSamuel.Camille;
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
            container.Register<SubredditService>(Lifestyle.Scoped);
            // above use db, below do not
            container.Register<RedditService>(Lifestyle.Singleton);
            container.Register(() => new RoleService(
                s["security.admins"].Split(',').Select(x => x.Trim()).ToList()), Lifestyle.Singleton);
            container.Register(() => RiotApi.NewInstance(
                new RiotApiConfig.Builder(s["riot.apiKey"])
                {
                    Retries = int.Parse(s["riot.maxAttempts"]) - 1
                }.Build()), Lifestyle.Singleton);

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
