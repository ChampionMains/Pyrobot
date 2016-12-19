using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;
using RiotSharp;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class RegistrationApiController : ApiController
    {
        protected RiotApi Riot { get; set; }
        protected SummonerService Summoners { get; set; }
        protected UserService Users { get; set; }
        protected ValidationService Validation { get; set; }
        protected WebJobService WebJob { get; set; }

        public RegistrationApiController(RiotApi riotService, SummonerService summonerService,
                UserService userService, ValidationService validationService, WebJobService webJobService)
        {
            Riot = riotService;
            Summoners = summonerService;
            Users = userService;
            Validation = validationService;
            WebJob = webJobService;
        }

        [HttpPost, Route("profile/api/summoner/register")]
        public async Task<IHttpActionResult> Register(SummonerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await Users.GetUserAsync();
                if (user == null)
                    return StatusCode(HttpStatusCode.Unauthorized);

                // Summoner MUST exist.
                var cacheKey = string.Concat(model.Region, ":", model.SummonerName).ToLowerInvariant();
                Region region;
                if (!Enum.TryParse(model.Region.ToLowerInvariant(), out region))
                    return BadRequest("Unknown region: " + model.Region);
                var summoner = await CacheUtil.GetItemAsync(cacheKey, () => Riot.GetSummonerAsync(region, model.SummonerName));
                if (summoner == null)
                    return Conflict("Summoner not found.");

                // Summoner MUST NOT be registered.
                if (await Summoners.IsSummonerRegisteredAsync(model.Region, model.SummonerName))
                    return Conflict("Summoner is already registered.");

                return Ok(new
                {
                    code = await Validation.GenerateAsync(User.Identity.Name, summoner.Id, model.Region, user.Name)
                });
            }
            catch (RiotSharpException e)
            {
                //TODO
                throw;
            }
        }

        [HttpPost, Route("profile/api/summoner/validate")]
        public async Task<IHttpActionResult> Validate(SummonerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await Users.GetUserAsync();
                if (user == null)
                    return StatusCode(HttpStatusCode.Unauthorized);

                Region region;
                if (!Enum.TryParse(model.Region.ToLowerInvariant(), out region))
                    return BadRequest("Invalid region.");

                // Summoner MUST exist.
                var riotSummoner = await Riot.GetSummonerAsync(region, model.SummonerName);
                if (riotSummoner == null)
                    return Conflict("Summoner not found.");

                // Summoner MUST NOT be registered.
                if (await Summoners.IsSummonerRegisteredAsync(model.Region, model.SummonerName))
                    return Conflict("Summoner is already registered.");

                var runePages = (await Riot.GetRunePagesAsync(region, new List<long>(1) { riotSummoner.Id } )).First().Value;
                var code = await Validation.GenerateAsync(User.Identity.Name, riotSummoner.Id, model.Region, user.Name);
                
                if (!runePages.Any(page => string.Equals(page.Name, code, StringComparison.OrdinalIgnoreCase)))
                    return StatusCode(HttpStatusCode.ExpectationFailed);

                // Create the data entity and associate it with the current user
                var currentSummoner = Summoners.AddSummoner(user.Id, riotSummoner.Id, model.Region, riotSummoner.Name, riotSummoner.ProfileIconId);
                var changes = await Summoners.SaveChangesAsync();

                // Send confirmation mail.
                //TODO
                Trace.WriteLine($"user.id={user.Id}, user.name={user.Name}, summoner.Id={currentSummoner.Id}");
                //BackgroundJob.Enqueue<ConfirmRegistrationMailJob>(job => job.Execute(user.Id, currentSummoner.Id));

                // Queue up the league update.
                await WebJob.QueueSummonerUpdate(currentSummoner.Id);

                return Ok();
            }
            catch (RiotSharpException e)
            {
                //TODO
                throw;
            }
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}
