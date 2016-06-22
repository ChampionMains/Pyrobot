using System.Web.Mvc;

namespace ChampionMains.Pyrobot.Areas.AdminPanel.Controllers
{
    [AdminAuthorize]
    public class AdminDefaultController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}