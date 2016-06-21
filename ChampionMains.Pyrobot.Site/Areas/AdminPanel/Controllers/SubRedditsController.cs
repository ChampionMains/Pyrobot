using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs.Areas.AdminPanel.Controllers
{
    [AdminAuthorize]
    public class SubRedditsController : ApiController
    {
        private readonly ISubRedditService _subReddits;

        public SubRedditsController(ISubRedditService subReddits)
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
