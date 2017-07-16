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

        public ProfileController(UserService users, RiotService riot, SummonerService summoners)
        {
            Users = users;
            Riot = riot;
            Summoners = summoners;
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