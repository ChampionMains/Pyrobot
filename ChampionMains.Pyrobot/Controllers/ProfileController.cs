using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;
using RiotSharp;
using RiotSharp.SummonerEndpoint;

namespace ChampionMains.Pyrobot.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        public RiotApi Riot { get; set; }
        public SummonerService Summoners { get; set; }
        public UserService Users { get; set; }
        public ProfileViewModel ViewModel { get; set; }

        public ProfileController(UserService userService, RiotApi riotService, SummonerService summonerService)
        {
            Users = userService;
            Riot = riotService;
            Summoners = summonerService;
        }

        public async Task<ActionResult> Index()
        {
            ViewModel = await CreateViewModelAsync();
            if (ViewModel.User == null)
            {
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("index", "login");
            }
            return View(ViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Register(SummonerModel model)
        {
            ViewModel = await CreateViewModelAsync();
            if (!ModelState.IsValid)
                return Error(ModelState);

            // Rule: Summoner must not be registered to a User.
            if (await Summoners.IsSummonerRegisteredAsync(model.Region, model.SummonerName))
                return Error("Summoner is already registered.");

            // Rule: Summoner must exist.
            var cacheKey = string.Concat(model.Region, ":", model.SummonerName).ToLowerInvariant();
            Region region;
            if (!Enum.TryParse(model.Region.ToLowerInvariant(), out region))
                return Error("Unknown region: " + model.Region);
            var summoner = await CacheUtil.GetItemAsync(cacheKey, () => Riot.GetSummonerAsync(region, model.SummonerName));

            if (summoner == null)
                return Error("Summoner not found.");
            return Success();
        }

        private async Task<ProfileViewModel> CreateViewModelAsync()
        {
            var viewModel = new ProfileViewModel
            {
                User = await Users.FindAsync(User.Identity.Name),
            };
            return viewModel;
        }


        private ActionResult Error(ModelStateDictionary modelState)
        {
            var errorStates = modelState.SelectMany(kvp => kvp.Value.Errors);
            return Error(errorStates.First().ErrorMessage);
        }

        private ActionResult Error(string errorMessage)
        {
            return RedirectToAction("Index", new {error = errorMessage});
        }

        private ActionResult Success()
        {
            return RedirectToAction("Index");
        }
    }
}