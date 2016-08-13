using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Queues;
using Microsoft.WindowsAzure.Storage.Queue;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace ChampionMains.Pyrobot.WebJob
{
    public sealed class ScopedQueueProcessorFactory : IQueueProcessorFactory
    {
        private readonly Container _container;

        public ScopedQueueProcessorFactory(Container container)
        {
            _container = container;
        }

        public QueueProcessor Create(QueueProcessorFactoryContext context)
        {
            return new ScopedQueueProcessor(context, _container);
        }

        private class ScopedQueueProcessor : QueueProcessor
        {
            private readonly Container _container;

            public ScopedQueueProcessor(QueueProcessorFactoryContext context, Container container) : base(context)
            {
                _container = container;
            }

            public override Task<bool> BeginProcessingMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken)
            {
                _container.BeginExecutionContextScope();
                return base.BeginProcessingMessageAsync(message, cancellationToken);
            }

            public override Task CompleteProcessingMessageAsync(CloudQueueMessage message, FunctionResult result,
                CancellationToken cancellationToken)
            {
                _container.GetCurrentExecutionContextScope()?.Dispose();
                return base.CompleteProcessingMessageAsync(message, result, cancellationToken);
            }
        }
    }
}
