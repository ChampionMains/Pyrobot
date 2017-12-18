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
using ChampionMains.Pyrobot.Util;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class RegistrationApiController : ApiController
    {
        /// <summary>
        /// Public profile icons range from id 0-28.
        /// </summary>
        private const int ProfileIconRange = 29;

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

                // We always pull from Riot in order to get the latest profile icon. (Within 2 min).
                var riotSummoner = await FindSummonerAsync(model.Region, model.SummonerName);
                if (riotSummoner == null)
                    return Conflict("Summoner not found.");
                
                var profileIcon = ThreadRandom.NextExcluding(ProfileIconRange, riotSummoner.ProfileIconId);

                return Ok(new
                {
                    profileIcon,
                    token = Validation.GenerateToken(User.Identity.Name, riotSummoner.Id, model.Region, user.Name, profileIcon)
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

                // We always pull from Riot in order to get the latest profile icon. With force.
                var riotSummoner = await FindSummonerAsync(model.Region, model.SummonerName, true);
                if (riotSummoner == null)
                    return Conflict("Summoner not found.");

                // Check if token is valid and user has updated their profile icon.
                if (!Validation.ValidateToken(model.Token, User.Identity.Name, riotSummoner.Id, model.Region, user.Name,
                    riotSummoner.ProfileIconId))
                {
                    return StatusCode(HttpStatusCode.ExpectationFailed);
                }

                // If Summoner is already in DB, we transfer it. Make sure to use summoner ID in case a name change has happened.
                var dbSummoner = await Summoners.FindSummonerAsync(model.Region, riotSummoner.Id);

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

        private Task<Summoner> FindSummonerAsync(string region, string summonerName, bool force = false)
        {
            return CacheUtil.GetItemAsync($"{region}:{summonerName}".ToLowerInvariant(),
                () => Riot.GetSummoner(region, summonerName), force);
        }
    }
}
