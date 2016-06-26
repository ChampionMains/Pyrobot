using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class SubRedditApiController : ApiController
    {
        private readonly FlairService _flair;
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly WebJobService _webJob;
        private readonly SubRedditService _subReddits;

        public SubRedditApiController(UserService users, SummonerService summoners, FlairService flair, WebJobService webJob, SubRedditService subReddits)
        {
            _users = users;
            _summoners = summoners;
            _flair = flair;
            _webJob = webJob;
            _subReddits = subReddits;
        }

        [HttpGet]
        [Route("profile/api/subreddits")]
        public async Task<IEnumerable<object>> GetSubReddits()
        {
            var user = await _users.GetUserAsync();

            var subReddits = await _subReddits.GetAllAsync();

            return subReddits.Where(subReddit => !subReddit.AdminOnly || user.IsAdmin).Select(subReddit => new
            {
                id = subReddit.Id,
                name = subReddit.Name,
                //totalPoints = user.Summoners.Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == subReddit.ChampionId)?.Points ?? 0).Sum(),
                //level = user.Summoners.Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == subReddit.ChampionId)?.Level ?? 0).Max(),
                champion = new
                {
                    name = subReddit.Champion.Name,
                    identifier = subReddit.Champion.Identifier,
                    id = subReddit.ChampionId
                },
                championMasteryEnabled = subReddit.ChampionMasteryEnabled,
                rankEnabled = subReddit.RankEnabled,
                adminOnly = subReddit.AdminOnly
            });
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}

