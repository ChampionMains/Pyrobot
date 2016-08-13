using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;

namespace ChampionMains.Pyrobot.Areas.AdminPanel.Controllers
{
    [AdminAuthorize]
    public class SubscriptionsController : ApiController
    {
        //private readonly RedditService _reddit;
        //private readonly SubredditService _subreddits;

        //public SubscriptionsController(SubredditService subreddits, RedditService reddit)
        //{
        //    _subreddits = subreddits;
        //    _reddit = reddit;
        //}

        //[HttpGet, Route("adminPanel/api/moderatorOf")]
        //public async Task<IHttpActionResult> GetModeratorOf()
        //{
        //    try
        //    {
        //        return Ok(await _reddit.GetSubredditsAsync(SubredditKind.Moderator));
        //    }
        //    catch (HttpRequestException)
        //    {
        //        return Content(HttpStatusCode.Conflict, "Error communicating with Reddit.");
        //    }
        //}

        //[HttpGet, Route("adminPanel/api/subscriptions")]
        //public async Task<IHttpActionResult> Get()
        //{
        //    var entries = await _subreddits.GetAllAsync();
        //    return Ok(from e in entries
        //              orderby e.Name
        //              select new { name = e.Name, status = "Linked" });
        //}

        //public class SubscribeDto
        //{
        //    [Required]
        //    public string Name { get; set; }
        //}

        //[HttpPost, Route("adminPanel/api/subscribe")]
        //public async Task<IHttpActionResult> Subscribe(SubscribeDto model)
        //{
        //    if(!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var moderatorSubs = await _reddit.GetSubredditsAsync(SubredditKind.Moderator);
        //    var currentSubs = await _subreddits.GetAllAsync();

        //    if (!moderatorSubs.Contains(model.Name, StringComparer.InvariantCultureIgnoreCase))
        //    {
        //        return Conflict("Moderator trait is required.");
        //    }

        //    if (currentSubs.Any(s => s.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        return Conflict("That Subreddit is already linked.");
        //    }

        //    if (!await _subreddits.AddAsync(model.Name))
        //    {
        //        return Conflict("Error linking Subreddit.");
        //    }

        //    return Ok();
        //}

        ///*
        //[HttpPost, Route("adminPanel/api/unsubscribe")]
        //public async Task<IHttpActionResult> Unsubscribe(SubscribeDto model)
        //{
            
        //} 
        //*/

        //private IHttpActionResult Conflict(string message)
        //{
        //    return Content(HttpStatusCode.Conflict, message);
        //}
    }
}
