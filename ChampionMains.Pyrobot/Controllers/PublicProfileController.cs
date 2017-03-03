using System.Web.Mvc;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [RoutePrefix("u")]
    public class PubilcProfileController : Controller
    {
        public RiotService Riot { get; set; }
        public SummonerService Summoners { get; set; }
        public UserService Users { get; set; }
        public ProfileViewModel ViewModel { get; set; }

        public PubilcProfileController(UserService userService, RiotService riotService, SummonerService summonerService)
        {
            Users = userService;
            Riot = riotService;
            Summoners = summonerService;
        }


        //[Route("{userName}")]
        //public ActionResult Detail(string userName)
        //{
        //    return View();
        //}
    }
}