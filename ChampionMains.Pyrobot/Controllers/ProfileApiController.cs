using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.WebJob;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    [WebApiAuthorize]
    public class ProfileApiController : ApiController
    {
        private readonly SummonerService _summonerService;
        private readonly UserService _userService;
        private readonly WebJobService _webJobService;
        private readonly FlairService _flairService;
        private readonly UnitOfWork _unitOfWork;

        private readonly TimeSpan _riotUpdateMinStaleTime;

        public ProfileApiController(UserService userService, SummonerService summonerService, WebJobService webJobService,
            FlairService flairService, UnitOfWork unitOfWork, ApplicationConfiguration config)
        {
            _userService = userService;
            _summonerService = summonerService;
            _webJobService = webJobService;
            _unitOfWork = unitOfWork;
            _flairService = flairService;

            _riotUpdateMinStaleTime = config.RiotUpdateMin;
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/delete")]
        public async Task<IHttpActionResult> DeleteSummoner(SummonerDataViewModel model)
        {
            var user = await _userService.GetUserAsync();
            var summoner = _unitOfWork.Summoners.Find(model.Id);

            if (summoner == null || summoner.UserId != user.Id)
                return Conflict("Summoner not found.");

            if (!await _summonerService.RemoveAsync(model.Id))
                return Conflict("Unable to remove summoner.");

            return Ok();
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/summoner/refresh")]
        public async Task<IHttpActionResult> RefreshSummoner(SummonerDataViewModel model)
        {
            var user = await _userService.GetUserAsync();
            var summoner = _unitOfWork.Summoners.Find(model.Id);

            if (summoner == null || summoner.UserId != user.Id)
                return Conflict("Summoner not found.");

            if (summoner.LastUpdate > DateTimeOffset.Now - _riotUpdateMinStaleTime)
                return TooManyRequests($"Summoner updated recently (less than {_riotUpdateMinStaleTime})");

            await _webJobService.QueueSummonerUpdate(summoner.Id);
            return Ok();
        }

        [HttpPost]
        [ValidateModel]
        [Route("profile/api/subreddit/update")]
        public async Task<IHttpActionResult> UpdateSubreddit(SubredditUserDataViewModel model)
        {
            var user = await _userService.GetUserAsync();

            var changed = await _flairService.UpdateSubredditUserFlair(user.Id, model.SubredditId, model.RankEnabled,
                model.ChampionMasteryEnabled, model.PrestigeEnabled, model.FlairText);

            if (!changed)
                return Conflict($"Subreddit (id {model.SubredditId}) does not exist");

            await _webJobService.QueueFlairUpdate(new FlairUpdateMessage()
            {
                UserId = user.Id,
                SubredditId = model.SubredditId,
                RankEnabled = model.RankEnabled,
                ChampionMasteryEnabled = model.ChampionMasteryEnabled,
                PrestigeEnabled = model.PrestigeEnabled,
                FlairText = model.FlairText, // can be null
            });

            return Ok();
        }

        [HttpGet]
        [Route("profile/api/data")]
        public async Task<object> GetData()
        {
            var user = await _userService.GetUserAsync();
            
            var summoners = _unitOfWork.Summoners.Where(s => s.UserId == user.Id)
                .Include(s => s.Rank).Include(s => s.ChampionMasteries)
                .ToDictionary(s => s.Id, s => new SummonerDataViewModel
                {
                    Id = s.Id,
                    Region = s.Region.ToUpperInvariant(),
                    Name = s.Name,
                    ProfileIcon = s.ProfileIconId,
                    Rank = RankUtil.Stringify(s.Rank),
                    Tier = s.Rank.Tier,
                    TierString = ((Tier) s.Rank.Tier).ToString().ToLower(),
                    Division = s.Rank.Division,
                    TotalPoints = s.ChampionMasteries.Select(cm => cm.Points).DefaultIfEmpty().Sum(),
                    LastUpdate = s.LastUpdate
                });

            var masteries = user.Summoners.SelectMany(s => s.ChampionMasteries).ToList();
            // TODO: encapsulate unit of work
            var champions = _unitOfWork.Champions.ToDictionary(c => c.Id, c =>
            {
                var champion = masteries.Where(m => m.ChampionId == c.Id).Select(m => new ChampionMasteryDataViewModel
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

            var subreddits = _unitOfWork.Subreddits.Where(r => !r.AdminOnly || user.IsAdmin)
                .ToList().ToDictionary(r => r.Id, r =>
                {
                    var subredditUserData = _unitOfWork.SubredditUserFlairs.FirstOrDefault(f => f.SubredditId == r.Id);
                    var flair = new SubredditUserDataViewModel
                    {
                        SubredditId = r.Id,
                    };

                    if (subredditUserData != null)
                    {
                        flair.RankEnabled = subredditUserData.RankEnabled;
                        flair.ChampionMasteryEnabled = subredditUserData.ChampionMasteryEnabled;
                        flair.PrestigeEnabled = subredditUserData.PrestigeEnabled;
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
                });

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

        private IHttpActionResult TooManyRequests(string message)
        {
            return Content((HttpStatusCode) 429, new HttpError(message));
        }
    }
}
