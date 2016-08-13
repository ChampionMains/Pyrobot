using System.Configuration;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Startup;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace ChampionMains.Pyrobot
{
    public class SimpleInjectorConfig
    {
        public static void Register(Container container)
        {
            var s = ConfigurationManager.AppSettings;

            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            // MVC controllers
            container.RegisterMvcControllers(typeof(MvcApplication).Assembly);
            container.RegisterMvcIntegratedFilterProvider();

            // Web API
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration, typeof(MvcApplication).Assembly);

            container.Register(() => new WebJobService(
                s["AzureWebJobsStorage"],
                s["webjob.wakeup.username"],
                s["webjob.wakeup.password"],
                s["webjob.wakeup.url"],
                s["userAgent"]), Lifestyle.Singleton);

            SharedSimpleInjectorConfig.Configure(container, s);
            

            // Replace the MVC dependency resolver
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            // Replace the WebAPI dependency resolver
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}