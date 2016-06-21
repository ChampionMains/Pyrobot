using System.Web.Mvc;

namespace Hallam.RedditRankedFlairs.Areas.AdminPanel.Controllers
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