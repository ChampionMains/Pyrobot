using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Models;
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

            Subreddit subreddit;
            using (var db = new UnitOfWork())
                subreddit = await db.Subreddits.FirstOrDefaultAsync(s => subredditName == s.Name);

            if (null == subreddit) // || subreddit.MissingMod)
                return HttpNotFound();
            
            var userName = userIdentity.Name;
            var key = $"isMod:{subreddit.Name}:{userName}";

            var reddit = _reddit;
            var subredditMods = await CacheUtil.GetItemAsync(key,
                async () => await reddit.GetSubredditModsAsync(subredditName, new[] {"config", "flair"}),
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
