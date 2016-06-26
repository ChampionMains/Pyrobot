using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
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
            var builder = new ContainerBuilder();
            //// MVC controllers
            //builder.RegisterControllers(typeof(MvcApplication).Assembly);

            //// Web API
            //builder.RegisterApiControllers(typeof(MvcApplication).Assembly);
            //builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);

            //// DI model binders
            //builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            //builder.RegisterModelBinderProvider();

            //// Web abstractions
            //builder.RegisterModule<AutofacWebTypesModule>();

            //// Property injection in view pages
            //builder.RegisterSource(new ViewRegistrationSource());

            //// Property injection in action filters
            //builder.RegisterFilterProvider();

            // Config
            var s = ConfigurationManager.AppSettings;

            builder.Register(context => new ApplicationConfiguration
            {
                FlairBotVersion = s["bot.version"],
                LeagueDataStaleTime = TimeSpan.Parse(s["website.leagueUpdateStaleTime"])
            }).SingleInstance();

            // Services
            builder.RegisterType(typeof(RoleService)).SingleInstance();
            builder.RegisterType(typeof(UserService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(SummonerService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(SubRedditService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(RedditService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(FlairService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(ValidationService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(LeagueUpdateService)).InstancePerLifetimeScope();
            builder.Register(context => new RiotService
            {
                WebRequester = new RiotWebRequester
                {
                    ApiKey = s["riot.apiKey"],
                    MaxAttempts = int.Parse(s["riot.maxAttempts"]),
                    MaxRequestsPer10Seconds = int.Parse(s["riot.maxRequestsPer10Seconds"]),
                    RetryInterval = TimeSpan.Parse(s["riot.retryInterval"])
                }
            }).SingleInstance();

            // Reddit WebRequester
            builder.Register(context => new RedditWebRequester(s["reddit.script.clientId"],
                    s["reddit.script.clientSecret"],
                    s["reddit.modUserName"],
                    s["reddit.modPassword"])).SingleInstance();

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
            builder.RegisterType(typeof(SummonerUpdateJob)).InstancePerLifetimeScope();

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
