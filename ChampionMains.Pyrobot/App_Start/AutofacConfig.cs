using System;
using System.Configuration;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Jobs;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using ChampionMains.Pyrobot.Services.Riot;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace ChampionMains.Pyrobot
{
    public class AutofacConfig
    {
        public static void Register(ContainerBuilder builder)
        {
            var s = ConfigurationManager.AppSettings;

            // MVC controllers
            builder.RegisterControllers(typeof (MvcApplication).Assembly);

            // Web API
            builder.RegisterApiControllers(typeof (MvcApplication).Assembly);
            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);

            // DI model binders
            builder.RegisterModelBinders(typeof (MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // Web abstractions
            builder.RegisterModule<AutofacWebTypesModule>();
            
            // Property injection in view pages
            builder.RegisterSource(new ViewRegistrationSource());

            // Property injection in action filters
            builder.RegisterFilterProvider();

            // Config

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
            builder.Register(context => new WebJobService(s["AzureWebJobsStorage"]));
            builder.Register(context => new RedditWebRequester(s["reddit.script.clientId"], 
                    s["reddit.script.clientSecret"],
                    s["reddit.modUserName"], 
                    s["reddit.modPassword"])).SingleInstance();

            //// Jobs
            //builder.RegisterType(typeof (SummonerUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof (BulkFlairUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof (FlairUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof (BulkLeagueUpdateJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof (ConfirmRegistrationMailJob)).InstancePerLifetimeScope();
            //builder.RegisterType(typeof (ConfirmFlairUpdatedMailJob)).InstancePerLifetimeScope();

            // Data persistance
            builder.RegisterType(typeof(UnitOfWork)).InstancePerLifetimeScope();

            Configure(builder.Build());
        }

        private static void Configure(IContainer container)
        {
            // Replace the MVC dependency resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Replace the WebAPI dependency resolver
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // Replace the Hang-fire activator
            //ConfigureHang-fire(Hang-fire.GlobalConfiguration.Configuration, container);
        }

        //private static void ConfigureHang-fire(IGlobalConfiguration config, IContainer container)
        //{
        //    config.UseAutofacActivator(container);
        //}
    }
}