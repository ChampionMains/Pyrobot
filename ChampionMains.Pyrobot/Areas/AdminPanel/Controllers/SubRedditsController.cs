using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Areas.AdminPanel.Controllers
{
    [AdminAuthorize]
    public class SubRedditsController : ApiController
    {
        private readonly SubRedditService _subReddits;

        public SubRedditsController(SubRedditService subReddits)
        {
            _subReddits = subReddits;
        }

        [HttpGet, Route("adminPanel/subReddits")]
        public async Task<IHttpActionResult> Get()
        {
            var items = await _subReddits.GetAllAsync();
            return Ok(from i in items
                      orderby i.Name
                      select new {name = i.Name, status = "Connected"});
        }
    }
}
