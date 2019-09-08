using System;
using System.Configuration;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Startup;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace ChampionMains.Pyrobot.Test
{
    public static class TestSimpleInjector
    {
        private static Container _container;
        
        public static TService GetInstance<TService>() where TService : class
        {
            if (null == _container)
            {
                _container = new Container();

                _container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

                var s = ConfigurationManager.AppSettings;

                SharedSimpleInjectorConfig.Configure(_container, s);

                _container.Verify();
            }
            return _container.GetInstance<TService>();
        }
    }
}
