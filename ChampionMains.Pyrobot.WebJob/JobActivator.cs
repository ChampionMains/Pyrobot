using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
