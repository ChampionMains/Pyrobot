using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Services.Reddit;

namespace ChampionMains.Pyrobot.Controllers
{
    [RoutePrefix("m/{" + SubredditNameParamterName + "}")]
    public class ModPanelController : Controller
    {
        private const string SubredditNameParamterName = "subredditName";

        private readonly RedditService _reddit;

        public ModPanelController(RedditService reddit)
        {
            _reddit = reddit;
        }

        protected async Task<ActionResult> ValidateUserIsModerator()
        {
            var context = HttpContext;
//            base.OnActionExecuting(context);

            var subredditName = context.Request.RequestContext.RouteData.Values[SubredditNameParamterName]?.ToString();
            var userIdentity = context.User.Identity;

            if (string.IsNullOrWhiteSpace(subredditName) || !userIdentity.IsAuthenticated)
                return HttpNotFound();

            var userName = userIdentity.Name;
            var key = $"isMod:{subredditName}:{userName}";
            var subredditMods = await CacheUtil.GetItemAsync(key,
                async () => await _reddit.GetSubredditModsAsync(subredditName, new[] {"config", "flair"}),
                expirery: TimeSpan.FromMinutes(10));

            if (!subredditMods.Contains(userName))
                return HttpNotFound();

            return null;
        }

        [Route("")]
        public async Task<ActionResult> Index()
        {
            var validation = await ValidateUserIsModerator();
            if (null != validation)
                return validation;
            return View();
        }
    }
}
