using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class ProfileApiController : ApiController
    {
        private readonly FlairService _flair;
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly WebJobService _webJob;

        public ProfileApiController(UserService users, SummonerService summoners, FlairService flair, WebJobService webJob)
        {
            _users = users;
            _summoners = summoners;
            _flair = flair;
            _webJob = webJob;
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/delete")]
        public async Task<IHttpActionResult> DeleteSummoner(SummonerModel model)
        {
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

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/refresh")]
        public async Task<IHttpActionResult> RefreshSummoner(SummonerModel model)
        {
            var user = await _users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(DbUtil.CreateComparer(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            await _webJob.QueueSummonerUpdate(summoner.Id);
            return Ok();
        }

        [HttpGet]
        [Route("profile/api/summoners")]
        public async Task<IEnumerable<object>> GetSummoners()
        {
            var user = await _users.GetUserAsync();
            return user.Summoners.Select(summoner => new
            {
                id = summoner.Id,
                region = summoner.Region.ToUpperInvariant(),
                summonerName = summoner.Name,
                rank = RankUtil.Stringify(summoner.Rank),
                tier = summoner.Rank.Tier,
                tierString = ((Tier) summoner.Rank.Tier).ToString().ToLower(),
                division = summoner.Rank.Division,
                totalPoints = summoner.ChampionMasteries.Select(cm => cm.Points).Sum(),
                champions = summoner.ChampionMasteries.ToDictionary(cm => cm.ChampionId, champ => new
                {
                    name = champ.Champion.Name,
                    id = champ.ChampionId,
                    identifier = champ.Champion.Identifier,
                    points = champ.Points,
                    level = champ.Level,
                    prestige = RankUtil.GetprestigeLevel(champ.Points)
                })
            });
        }

        [HttpGet]
        [Route("profile/api/prestiges")]
        public IEnumerable<int> GetPrestiges()
        {
            return RankUtil.PrestigeLevels;
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}
