using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Startup;
using ChampionMains.Pyrobot.WebJob.Jobs;
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

            container.Register(() => new WebJobConfiguration
            {
                TimeoutBulkUpdate = TimeSpan.Parse(s["webjob.timeout.bulkUpdate"])
            }, Lifestyle.Singleton);

            // Jobs
            container.Register<SummonerUpdateJob>(Lifestyle.Transient);
            container.Register<FlairUpdateJob>(Lifestyle.Transient);
            container.Register<BulkUpdateJob>(Lifestyle.Transient);

            SharedSimpleInjectorConfig.Configure(container, s);

            // begin default scope (webjobs calls will have own scope per-request)
            // for calling from scm
            container.BeginExecutionContextScope();

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
                return (T)_container.GetInstance(typeof(T));
            }
        }
    }
}
