using System;
using System.Collections.Specialized;
using System.Linq;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using ChampionMains.Pyrobot.Services.Riot;
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
            if (s["website.hmacKey"]?.Length < 32)
                throw new ArgumentException("website.hmacKey has insufficient length or is null.");
            container.Register(() => new RoleService(
                s["security.admins"].Split(',').Select(x => x.Trim()).ToList()), Lifestyle.Singleton);
            container.Register(() => new RiotService
            {
                WebRequester = new RiotWebRequester(s["riot.rateLimit"])
                {
                    ApiKey = s["riot.apiKey"],
                    MaxAttempts = int.Parse(s["riot.maxAttempts"]),
                    RetryInterval = TimeSpan.Parse(s["riot.retryInterval"])
                }
            }, Lifestyle.Singleton);

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
