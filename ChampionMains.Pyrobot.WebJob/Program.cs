using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Autofac;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Jobs;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using ChampionMains.Pyrobot.Services.Riot;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            // do database migrations
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UnitOfWork, Data.Migrations.Configuration>());

            // dependency injection
            var builder = new ContainerBuilder();

            // Config
            var s = ConfigurationManager.AppSettings;

            builder.Register(context => new ApplicationConfiguration
            {
                FlairBotVersion = s["bot.version"],
                RiotUpdateMin = TimeSpan.Parse(s["website.riotUpdateMin"]),
                RiotUpdateMax = TimeSpan.Parse(s["website.riotUpdateMax"]),
                FlairUpdateMin = TimeSpan.Parse(s["website.flairUpdateMin"]),
                FlairUpdateMax = TimeSpan.Parse(s["website.flairUpdateMax"])
            }).SingleInstance();

            // Services
            builder.RegisterType(typeof(UserService)).SingleInstance();
            builder.RegisterType(typeof(SummonerService)).SingleInstance();
            builder.RegisterType(typeof(SubredditService)).SingleInstance();
            builder.RegisterType(typeof(RedditService)).SingleInstance();
            builder.RegisterType(typeof(ValidationService)).SingleInstance();
            builder.RegisterType(typeof(FlairService)).SingleInstance();
            builder.Register(context => new RoleService(
                    ConfigurationManager.AppSettings["security.admins"].Split(',').Select(x => x.Trim()).ToList())
            ).SingleInstance();
            builder.Register(context => new RiotService
            {
                WebRequester = new RiotWebRequester(s["riot.rateLimit"])
                {
                    ApiKey = s["riot.apiKey"],
                    MaxAttempts = int.Parse(s["riot.maxAttempts"]),
                    RetryInterval = TimeSpan.Parse(s["riot.retryInterval"])
                }
            }).SingleInstance();

            // Reddit WebRequester
            builder.Register(context => new RedditWebRequester(
                    s["reddit.script.clientId"],
                    s["reddit.script.clientSecret"],
                    s["reddit.modUserName"],
                    s["reddit.modPassword"],
                    s["userAgent"])).SingleInstance();

            //// Jobs
            //builder.RegisterType(typeof(SummonerUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof(BulkFlairUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof(FlairUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof(BulkLeagueUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof(ConfirmRegistrationMailJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof(ConfirmFlairUpdatedMailJob)).InstancePerLifetimeScope();

            // Data persistance
            builder.RegisterType(typeof(UnitOfWork)).InstancePerDependency();

            // Jobs
            builder.RegisterType(typeof(SummonerUpdateJob)).InstancePerDependency();
            builder.RegisterType(typeof(FlairUpdateJob)).InstancePerDependency();
            builder.RegisterType(typeof(BulkUpdateJob)).InstancePerDependency();

            // Configure JobHost
            var config = new JobHostConfiguration
            {
                JobActivator = new JobActivator(builder.Build()),
                Queues =
                {
                    MaxDequeueCount = int.Parse(s["webjob.maxAttempts"]),
                    MaxPollingInterval = TimeSpan.Parse(s["webjob.pollingInterval"])
                }
            };
            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
