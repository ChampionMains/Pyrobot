using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ChampionMains.Pyrobot.Services
{
    public class WebJobService
    {
        private readonly CloudQueueClient _queueClient;
        private readonly HttpClient _httpClient; // TODO DI.

        private readonly string _wakeupUsername;
        private readonly string _wakeupPassword;
        private readonly string _wakeupUrl;
        private readonly string _userAgent;

        public WebJobService(string connectionString, string wakeupUsername, string wakeupPassword, string wakeupUrl, string userAgent)
        {
            // Open storage account using credentials from .cscfg file.
            // Get context object for working with queues, and
            // set a default retry policy appropriate for a web user interface.
            _queueClient = CloudStorageAccount.Parse(connectionString).CreateCloudQueueClient();
            _queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);
            _httpClient = new HttpClient();

            _wakeupUsername = wakeupUsername;
            _wakeupPassword = wakeupPassword;
            _wakeupUrl = wakeupUrl;
            _userAgent = Uri.EscapeDataString(userAgent);
        }

        public async Task QueueSummonerUpdate(int id)
        {
            await Queue(WebJobQueue.SummonerUpdate, id.ToString());
        }

        public async Task QueueFlairUpdate(FlairUpdateMessage data)
        {
            await Queue(WebJobQueue.FlairUpdate, Json.Encode(data));
        }

        public async Task QueueBulkUpdate(string tag = "")
        {
            await Queue(WebJobQueue.BulkUpdate, tag);
        }

        public async Task QueueSubredditCssUpdate(string tag = "")
        {
            await Queue(WebJobQueue.SubredditCssUpdate, tag);
        }

        private async Task Queue(string containerName, string message)
        {
            await WakeupWebJob();

            var queue = _queueClient.GetQueueReference(containerName);
            await queue.CreateIfNotExistsAsync();

            var queueMessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queueMessage);
        }

        private async Task WakeupWebJob()
        {
            if (string.IsNullOrWhiteSpace(_wakeupUsername)
                || string.IsNullOrWhiteSpace(_wakeupPassword)
                || string.IsNullOrWhiteSpace(_wakeupUrl))
                return;

            var message = new HttpRequestMessage(HttpMethod.Post, _wakeupUrl);
            message.Headers.Add("User-Agent", _userAgent);
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(_wakeupUsername + ":" + _wakeupPassword));
            message.Headers.Add("Authorization", "Basic " + authToken);
            await _httpClient.SendAsync(message);
        }
    }
}
