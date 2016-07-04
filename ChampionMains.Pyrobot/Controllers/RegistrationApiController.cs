using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Riot;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class RegistrationApiController : ApiController
    {
        protected RiotService Riot { get; set; }
        protected SummonerService Summoners { get; set; }
        protected UserService Users { get; set; }
        protected ValidationService Validation { get; set; }
        protected WebJobService WebJob { get; set; }

        public RegistrationApiController(RiotService riotService, SummonerService summonerService,
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
                var summoner = await FindSummonerAsync(model.Region, model.SummonerName);
                if (summoner == null)
                    return Conflict("Summoner not found.");

                // Summoner MUST NOT be registered.
                if (await Summoners.IsSummonerRegistered(model.Region, model.SummonerName))
                    return Conflict("Summoner is already registered.");

                return Ok(new
                {
                    code = await Validation.GenerateAsync(User.Identity.Name, summoner.Id, model.Region, user.Name)
                });
            }
            catch (RiotHttpException e)
            {
                switch ((int) e.StatusCode)
                {
                    case 500:
                    case 503:
                        return Conflict("Error communicating with Riot (Service unavailable)");
                }
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

                // Summoner MUST exist.
                var riotSummoner = await Riot.FindSummonerAsync(model.Region, model.SummonerName);
                if (riotSummoner == null)
                    return Conflict("Summoner not found.");

                // Summoner MUST NOT be registered.
                if (await Summoners.IsSummonerRegistered(model.Region, model.SummonerName))
                    return Conflict("Summoner is already registered.");

                var runePages = await Riot.GetRunePagesAsync(model.Region, riotSummoner.Id);
                var code = await Validation.GenerateAsync(User.Identity.Name, riotSummoner.Id, model.Region, user.Name);
                
                if (!runePages.Any(page => string.Equals(page.Name, code, StringComparison.InvariantCultureIgnoreCase)))
                    return StatusCode(HttpStatusCode.ExpectationFailed);

                // Create the data entity and associate it with the current user
                var currentSummoner =
                    await Summoners.AddOrUpdateSummonerAsync(user, riotSummoner.Id, model.Region, riotSummoner.Name, riotSummoner.ProfileIconId);

                // Send confirmation mail.
                //TODO
                Trace.WriteLine($"user.id={user.Id}, user.name={user.Name}, summoner.Id={currentSummoner.Id}");
                //BackgroundJob.Enqueue<ConfirmRegistrationMailJob>(job => job.Execute(user.Id, currentSummoner.Id));

                // Queue up the league update.
                await WebJob.QueueSummonerUpdate(currentSummoner.Id);

                return Ok();
            }
            catch (RiotHttpException e)
            {
                switch ((int) e.StatusCode)
                {
                    case 500:
                    case 503:
                        return Conflict("Error communicating with Riot (Service unavailable)");
                }
                throw;
            }
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }

        private Task<Summoner> FindSummonerAsync(string region, string summonerName)
        {
            return CacheUtil.GetItemAsync($"{region}:{summonerName}".ToLowerInvariant(),
                () => Riot.FindSummonerAsync(region, summonerName));
        }
    }
}
