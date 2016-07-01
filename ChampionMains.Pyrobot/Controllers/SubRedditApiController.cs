using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
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
        private readonly FlairService _flair;
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly WebJobService _webJob;
        private readonly SubredditService _subreddits;

        public SubredditApiController(UserService users, SummonerService summoners, FlairService flair, WebJobService webJob, SubredditService subreddits)
        {
            _users = users;
            _summoners = summoners;
            _flair = flair;
            _webJob = webJob;
            _subreddits = subreddits;
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/subreddit/update")]
        public async Task<IHttpActionResult> UpdateSubReddit(SubredditUserDataViewModel model)
        {
            var user = await _users.GetUserAsync();

            if (!await _subreddits.UpdateSubRedditUser(user.Id, model.SubredditId, model.RankEnabled,
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

