using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ChampionMains.Pyrobot.Services
{
    public class WebJobService
    {
        private readonly CloudQueueClient _queueClient;

        public WebJobService(string connectionString)
        {
            // Open storage account using credentials from .cscfg file.
            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            _queueClient = CloudStorageAccount.Parse(connectionString).CreateCloudQueueClient();
            _queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
        }

        public async Task<CloudQueue> GetCreateQueueClient(string containerName)
        {

            // Get a reference to the queue.
            var queue = _queueClient.GetQueueReference(containerName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }
    }
}
