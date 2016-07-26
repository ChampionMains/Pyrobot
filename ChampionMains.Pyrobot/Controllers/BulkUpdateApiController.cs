using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    public class BulkUpdateApiController : ApiController
    {
        private readonly WebJobService _webJob;

        public BulkUpdateApiController(WebJobService webJob)
        {
            _webJob = webJob;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("trigger-bulk-update")]
        public async Task<bool> TriggerBulkUpdate()
        {
            await _webJob.QueueBulkUpdate();
            return true;
        }
    }
}
