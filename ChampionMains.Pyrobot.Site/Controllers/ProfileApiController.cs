using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [Authorize]
    public class ProfileApiController : ApiController
    {
        private readonly IFlairService _flair;
        private readonly ISummonerService _summoners;
        private readonly IUserService _users;

        public ProfileApiController(IUserService users, ISummonerService summoners, IFlairService flair)
        {
            _users = users;
            _summoners = summoners;
            _flair = flair;
        }

        [HttpPost, Route("profile/api/activate")]
        public async Task<IHttpActionResult> ActivateSummoner(SummonerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(DbUtil.CreateComparer(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            if (!await _summoners.SetActiveSummonerAsync(summoner))
            {
                return Conflict("Unable to activate summoner.");
            }

            if (!await _flair.SetUpdateFlagAsync(new[] {user}))
            {
                return Conflict("Unable to activate summoner.");
            }
            return Ok();
        }

        [HttpPost, Route("profile/api/delete")]
        public async Task<IHttpActionResult> DeleteSummoner(SummonerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(DbUtil.CreateComparer(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            if (await _summoners.RemoveAsync(model.Region, model.SummonerName))
            {
                return Ok();
            }

            if (!await _flair.SetUpdateFlagAsync(new[] {user}))
            {
                return Conflict("Unable to remove summoner.");
            }
            return Conflict("Unable to remove summoner.");
        }

        [HttpGet, Route("profile/api/summoners")]
        public async Task<IEnumerable<object>> GetSummoners()
        {
            var user = await _users.GetUserAsync();
            return user.Summoners.Select(summoner => new
            {
                region = summoner.Region.ToUpperInvariant(),
                summonerName = summoner.Name,
                league = LeagueUtil.Stringify(summoner.SummonerInfo),
                //active = summoner.IsActive
            });
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}
