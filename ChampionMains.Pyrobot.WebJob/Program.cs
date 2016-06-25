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

            builder.Register(context => new ApplicationConfiguration
            {
                FlairBotVersion = ConfigurationManager.AppSettings["bot.version"],
                LeagueDataStaleTime = TimeSpan.Parse(ConfigurationManager.AppSettings["website.leagueUpdateStaleTime"])
            }).SingleInstance();

            // Services
            builder.RegisterType(typeof(RoleService)).As(typeof(IRoleService)).SingleInstance();
            builder.RegisterType(typeof(UserService)).As(typeof(IUserService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(SummonerService)).As(typeof(ISummonerService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(SubRedditService)).As(typeof(ISubRedditService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(RedditService)).As(typeof(IRedditService), typeof(IRedditMessengerService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(FlairService)).As(typeof(IFlairService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(ValidationService)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(LeagueUpdateService))
                .As(typeof(ILeagueUpdateService))
                .InstancePerLifetimeScope();
            builder.Register(context => new RiotService
            {
                WebRequester = new RiotWebRequester
                {
                    ApiKey = ConfigurationManager.AppSettings["riot.apiKey"],
                    MaxAttempts = int.Parse(ConfigurationManager.AppSettings["riot.maxAttempts"]),
                    MaxRequestsPer10Seconds = int.Parse(ConfigurationManager.AppSettings["riot.maxRequestsPer10Seconds"]),
                    RetryInterval = TimeSpan.Parse(ConfigurationManager.AppSettings["riot.retryInterval"])
                }
            }).As(typeof(IRiotService)).SingleInstance();

            // Reddit WebRequester
            builder.Register(context =>
            {
                var s = ConfigurationManager.AppSettings;
                return new RedditWebRequester(s["reddit.script.clientId"],
                    s["reddit.script.clientSecret"],
                    s["reddit.modUserName"],
                    s["reddit.modPassword"]);
            }).SingleInstance();

            // Jobs
            builder.RegisterType(typeof(LeagueUpdateJob)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(BulkFlairUpdateJob)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(FlairUpdateJob)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(BulkLeagueUpdateJob)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(ConfirmRegistrationMailJob)).InstancePerLifetimeScope();
            builder.RegisterType(typeof(ConfirmFlairUpdatedMailJob)).InstancePerLifetimeScope();

            // Data persistance
            builder.RegisterType(typeof(UnitOfWork)).As(typeof(IUnitOfWork)).InstancePerLifetimeScope();

            
            // finally
            var config = new JobHostConfiguration
            {
                JobActivator = new JobActivator(builder.Build())
            };
            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
