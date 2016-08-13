using System;
using System.Configuration;
using System.Data.Entity;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Jobs;
using ChampionMains.Pyrobot.Startup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace ChampionMains.Pyrobot.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        private static void Main()
        {
            // do database migrations
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UnitOfWork, Data.Migrations.Configuration>());

            var s = ConfigurationManager.AppSettings;

            // dependency injection
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            // Jobs
            container.Register<SummonerUpdateJob>();
            container.Register<FlairUpdateJob>();
            container.Register<BulkUpdateJob>();

            SharedSimpleInjectorConfig.Configure(container, s);

            // Configure JobHost
            var jobHostConfiguration = new JobHostConfiguration
            {
                JobActivator = new SimpleInjectorJobActivator(container),
                Queues =
                {
                    MaxDequeueCount = int.Parse(s["webjob.maxAttempts"]),
                    MaxPollingInterval = TimeSpan.Parse(s["webjob.pollingInterval"]),
                    QueueProcessorFactory = new ScopedQueueProcessorFactory(container)
                }
            };
            var host = new JobHost(jobHostConfiguration);

            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        public class SimpleInjectorJobActivator : IJobActivator
        {
            private readonly Container _container;

            public SimpleInjectorJobActivator(Container container)
            {
                _container = container;
            }

            public T CreateInstance<T>()
            {
                return (T) _container.GetInstance(typeof(T));
            }
        }
    }
}
