using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    public class PeriodicUpdateApiController : ApiController
    {
        private readonly WebJobService _webJob;

        public PeriodicUpdateApiController(WebJobService webJob)
        {
            _webJob = webJob;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("trigger-nightly")]
        public async Task TriggerNightly()
        {
            await Task.Yield();
            throw new NotImplementedException();
            //await _webJob.QueueBulkUpdate();
        }
    }
}
