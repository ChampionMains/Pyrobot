using Autofac;
using Microsoft.Azure.WebJobs.Host;

namespace ChampionMains.Pyrobot.WebJob
{
    public class JobActivator : IJobActivator
    {
        private readonly IContainer _container;

        public JobActivator(IContainer container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
