using System.Threading.Tasks;
using System.Web.Http;
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

        [HttpPost]
        [Route("trigger-bulk-update")]
        public async Task<bool> TriggerBulkUpdate()
        {
            await _webJob.QueueBulkUpdate(Request.Headers.UserAgent.ToString());
            return true;
        }
    }
}
