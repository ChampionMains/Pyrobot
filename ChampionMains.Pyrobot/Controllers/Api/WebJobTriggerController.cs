using System;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers.Api
{
    public class WebJobTriggerController : ApiController
    {
        private readonly WebJobService _webJobService;

        public WebJobTriggerController(WebJobService webJobsService)
        {
            _webJobService = webJobsService;
        }

        [HttpPost]
        [Route("trigger-bulk-update")]
        public async Task<bool> TriggerBulkUpdate()
        {
            await _webJobService.QueueBulkUpdate(DateTime.UtcNow.ToString("s"));
            return true;
        }

        [HttpPost]
        [Route("trigger-subreddit-css-update")]
        public async Task<bool> TriggerSubredditCssUpdate()
        {
            await _webJobService.QueueSubredditCssUpdate(DateTime.UtcNow.ToString("s"));
            return true;
        }
    }
}
