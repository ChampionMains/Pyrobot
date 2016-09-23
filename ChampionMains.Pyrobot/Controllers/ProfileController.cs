using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [Authorize]
    [RoutePrefix("profile")]
    public class ProfileController : Controller
    {
        public RiotService Riot { get; set; }
        public SummonerService Summoners { get; set; }
        public UserService Users { get; set; }
        public ProfileViewModel ViewModel { get; set; }

        public ProfileController(UserService userService, RiotService riotService, SummonerService summonerService)
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

        public ActionResult FlairDisplay(string subreddit)
        {
            return View((object) subreddit);
        }

        [HttpPost]
        public async Task<ActionResult> Register(SummonerModel model)
        {
            ViewModel = await CreateViewModelAsync();
            if (!ModelState.IsValid)
            {
                return Error(ModelState);
            }

            // Rule: Summoner must not be registered to a User.
            if (await Summoners.IsSummonerRegisteredAsync(model.Region, model.SummonerName))
            {
                return Error("Summoner is already registered.");
            }

            // Rule: Summoner must exist.
            var cacheKey = string.Concat(model.Region, ":", model.SummonerName).ToLowerInvariant();
            var summoner = await CacheUtil.GetItemAsync(cacheKey,
                () => Riot.GetSummoner(model.Region, model.SummonerName));

            if (summoner == null)
            {
                return Error("Summoner not found.");
            }

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