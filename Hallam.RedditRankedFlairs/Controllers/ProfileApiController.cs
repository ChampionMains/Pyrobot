﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Hallam.RedditRankedFlairs.Models;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs.Controllers
{
    [Authorize]
    public class ProfileApiController : ApiController
    {
        public ISummonerService Summoners { get; set; }
        public IUserService Users { get; set; }

        public ProfileApiController(IUserService users, ISummonerService summoners)
        {
            Users = users;
            Summoners = summoners;
        }

        [HttpPost, Route("profile/api/activate")]
        public async Task<IHttpActionResult> ActivateSummoner(SummonerModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await Users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(DbUtil.CreateComparer(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            if (!await Summoners.SetActiveSummonerAsync(summoner))
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
            var user = await Users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(DbUtil.CreateComparer(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            if (await Summoners.RemoveAsync(model.Region, model.SummonerName))
            {
                return Ok();
            }
            return Conflict("Error removing summoner.");
        }

        [HttpGet, Route("profile/api/summoners")]
        public async Task<IEnumerable<object>> GetSummoners()
        {
            var user = await Users.GetUserAsync();
            return user.Summoners.Select(summoner => new
            {
                region = summoner.Region.ToUpperInvariant(),
                summonerName = summoner.Name,
                league = "",
                active = summoner.Id == user.ActiveSummoner.Id
            });
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}
