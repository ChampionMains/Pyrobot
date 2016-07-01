using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class ProfileApiController : ApiController
    {
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly SubredditService _subreddit;
        private readonly WebJobService _webJob;
        private readonly UnitOfWork _unitOfWork;

        public ProfileApiController(UserService users, SummonerService summoners, SubredditService subreddit, WebJobService webJob, UnitOfWork unitOfWork)
        {
            _users = users;
            _summoners = summoners;
            _subreddit = subreddit;
            _webJob = webJob;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/delete")]
        public async Task<IHttpActionResult> DeleteSummoner(SummonerDataViewModel model)
        {
            var user = await _users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(u => u.Id == model.Id);

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            if (await _summoners.RemoveAsync(model.Id))
            {
                return Ok();
            }
            return Conflict("Unable to remove summoner.");
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/refresh")]
        public async Task<IHttpActionResult> RefreshSummoner(SummonerDataViewModel model)
        {
            var user = await _users.GetUserAsync();
            var summoner = user.Summoners.FirstOrDefault(u => u.Id == model.Id);

            if (summoner == null)
            {
                return Conflict("Summoner not found.");
            }

            await _webJob.QueueSummonerUpdate(summoner.Id);
            return Ok();
        }

        [HttpGet]
        [Route("profile/api/data")]
        public async Task<object> GetData()
        {
            var user = await _users.GetUserAsync();

            var summoners = user.Summoners.OrderByDescending(s => s.Rank.Tier).Select(s => new SummonerDataViewModel()
            {
                Id = s.Id,
                Region = s.Region.ToUpperInvariant(),
                Name = s.Name,
                Rank = RankUtil.Stringify(s.Rank),
                Tier = s.Rank.Tier,
                TierString = ((Tier) s.Rank.Tier).ToString().ToLower(),
                Division = s.Rank.Division,
                TotalPoints = s.ChampionMasteries.Select(cm => cm.Points).DefaultIfEmpty().Sum()
            }).ToList();

            var masteries = user.Summoners.SelectMany(s => s.ChampionMasteries).ToList();
            // TODO: encapsulate unit of work
            var champions = _unitOfWork.Champions.ToDictionary(c => c.Id, c =>
            {
                var champion = masteries.Where(m => m.ChampionId == c.Id).Select(m => new ChampionMasteryDataViewModel()
                {

                    Points = m.Points,
                    Level = m.Level
                }).DefaultIfEmpty(new ChampionMasteryDataViewModel()).Aggregate((a, b) =>
                {
                    a.Points += b.Points;
                    if (b.Level > a.Level)
                        a.Level = b.Level;
                    return a;
                });
                champion.Id = c.Id;
                champion.Name = c.Name;
                champion.Identifier = c.Identifier;
                champion.Prestige = RankUtil.GetPrestigeLevel(champion.Points);
                champion.NextPrestige = RankUtil.GetNextPrestigeLevel(champion.Points);
                return champion;
            });

            var subreddits = (await _subreddit.GetAllAsync()).Where(r => !r.AdminOnly || user.IsAdmin).Select(r =>
            {
                var subredditUserData = user.SubredditUserFlairs.FirstOrDefault(f => f.SubredditId == r.Id);
                var flair = new SubredditUserDataViewModel()
                {
                    SubredditId = r.Id,
                };
                if (subredditUserData != null)
                {
                    flair.RankEnabled = subredditUserData.RankEnabled;
                    flair.ChampionMasteryEnabled = subredditUserData.ChampionMasteryEnabled;
                    flair.PrestigeEnabled = r.PrestigeEnabled;
                    flair.FlairText = subredditUserData.FlairText;
                }

                return new SubredditDataViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    ChampionId = r.ChampionId,
                    AdminOnly = r.AdminOnly,
                    RankEnabled = r.RankEnabled,
                    ChampionMasteryEnabled = r.ChampionMasteryEnabled,
                    PrestigeEnabled = r.PrestigeEnabled,
                    BindEnabled = r.BindEnabled,
                    Flair = flair
                };
            }).ToList();

            return new ApiDataViewModel
            {
                Summoners = summoners,
                Champions = champions,
                Subreddits = subreddits
            };
        }

        [HttpGet]
        [Route("profile/api/prestiges")]
        public IEnumerable<int> GetPrestiges()
        {
            return RankUtil.PrestigeLevels;
        }

        private IHttpActionResult Conflict(string message)
        {
            return Content(HttpStatusCode.Conflict, new HttpError(message));
        }
    }
}
