using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Data.WebJob;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class SubredditApiController : ApiController
    {
        private readonly UserService _users;
        private readonly FlairService _flairs;
        private readonly WebJobService _webJob;

        public SubredditApiController(UserService users, FlairService flairs, WebJobService webJob)
        {
            _users = users;
            _flairs = flairs;
            _webJob = webJob;
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/subreddit/update")]
        public async Task<IHttpActionResult> UpdateSubReddit(SubredditUserDataViewModel model)
        {
            var user = await _users.GetUserAsync();

            if (!await _flairs.UpdateSubredditUserFlair(user.Id, model.SubredditId, model.RankEnabled,
                model.ChampionMasteryEnabled, model.PrestigeEnabled, model.FlairText))
                return BadRequest("Request did not match database");

            await _webJob.QueueFlairUpdate(new FlairUpdateMessage()
            {
                UserId = user.Id,
                SubRedditId = model.SubredditId,
                RankEnabled = model.RankEnabled,
                ChampionMasteryEnabled = model.ChampionMasteryEnabled,
                PrestigeEnabled = model.PrestigeEnabled,
                FlairText = model.FlairText, // can be null
            });

            return Ok();
        }
    }
}

