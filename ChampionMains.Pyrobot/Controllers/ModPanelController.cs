using System.Web.Mvc;

namespace ChampionMains.Pyrobot.Controllers
{
    [RoutePrefix("m")]
    public class ModPanelController : Controller
    {
        [Route("asdf")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
