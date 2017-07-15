using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Riot;
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

                Summoner riotSummoner = null;
                // If Summoner is already in DB, we transfer it.
                var dbSummoner = await Summoners.FindSummonerAsync(model.Region, model.SummonerName);
                if (dbSummoner == null)
                {
                    // Otherwise, we check the summoner exists from the Riot API.
                    riotSummoner = await FindSummonerAsync(model.Region, model.SummonerName);
                    if (riotSummoner == null)
                        return Conflict("Summoner not found.");
                }

                return Ok(new
                {
                    code = await Validation.GenerateAsync(User.Identity.Name,
                        dbSummoner?.SummonerId ?? riotSummoner.Id, model.Region, user.Name)
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

                Summoner riotSummoner = null;
                // If Summoner is already in DB, we transfer it.
                var dbSummoner = await Summoners.FindSummonerAsync(model.Region, model.SummonerName);
                if (dbSummoner == null)
                {
                    // Otherwise, we check the summoner exists from the Riot API.
                    riotSummoner = await FindSummonerAsync(model.Region, model.SummonerName);
                    if (riotSummoner == null)
                        return Conflict("Summoner not found.");
                }

                var summonerId = dbSummoner?.SummonerId ?? riotSummoner.Id;

                var runePages = await Riot.GetRunePages(model.Region, summonerId);
                var code = await Validation.GenerateAsync(User.Identity.Name, summonerId, model.Region, user.Name);
                
                if (!runePages.Any(page => string.Equals(page.Name, code, StringComparison.OrdinalIgnoreCase)))
                    return StatusCode(HttpStatusCode.ExpectationFailed);

                if (dbSummoner != null)
                {
                    dbSummoner.User = user;
                }
                else
                {
                    // Create the data entity and associate it with the current user
                    dbSummoner = Summoners.AddSummoner(user.Id, riotSummoner.Id, model.Region,
                        riotSummoner.Name, riotSummoner.ProfileIconId);
                }

                var changes = await Summoners.SaveChangesAsync();

                // Send confirmation mail.
                //TODO
                Trace.WriteLine($"user.id={user.Id}, user.name={user.Name}, summoner.Id={dbSummoner.Id}");
                //BackgroundJob.Enqueue<ConfirmRegistrationMailJob>(job => job.Execute(user.Id, currentSummoner.Id));

                // Queue up the league update.
                await WebJob.QueueSummonerUpdate(dbSummoner.Id);

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
                () => Riot.GetSummoner(region, summonerName));
        }
    }
}
