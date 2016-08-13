using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Areas.AdminPanel.Controllers
{
    [AdminAuthorize]
    public class SubredditsController : ApiController
    {
        //private readonly SubredditService _subreddits;

        //public SubredditsController(SubredditService subreddits)
        //{
        //    _subreddits = subreddits;
        //}

        //[HttpGet, Route("adminPanel/subreddits")]
        //public async Task<IHttpActionResult> Get()
        //{
        //    var items = await _subreddits.GetAllAsync();
        //    return Ok(from i in items
        //              orderby i.Name
        //              select new {name = i.Name, status = "Connected"});
        //}
    }
}
