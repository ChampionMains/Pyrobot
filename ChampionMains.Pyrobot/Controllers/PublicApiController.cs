using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    public class PublicApiController : ApiController
    {
        private readonly WebJobService _webJob;
        private readonly UserService _users;
        private readonly UnitOfWork _unitOfWork;

        public PublicApiController(WebJobService webJob, UserService users, UnitOfWork unitOfWork)
        {
            _webJob = webJob;
            _users = users;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("trigger-bulk-update")]
        public async Task<bool> TriggerBulkUpdate()
        {
            await _webJob.QueueBulkUpdate(Request.Headers.UserAgent.ToString());
            return true;
        }

        [Route("api/user-summoners")]
        public async Task<List<SummonerInfoViewModel>> GetUserSummoners(string username)
        {
            var user = await _users.FindAsync(username);
            if (user == null)
                throw new HttpResponseException(HttpStatusCode.NoContent);

            return _unitOfWork.Summoners.Where(s => s.UserId == user.Id)
                .Select(s => new SummonerInfoViewModel
                {
                    Name = s.Name,
                    Region = s.Region,
                    Id = s.SummonerId
                }).ToList();
        }

        [Route("api/leaderboard")]
        public async Task<LeaderboardViewModel> GetLeaderboard(int championId, int count = 10, int minPoints = 0)
        {
            var entriesQuery = _unitOfWork.SummonerChampionMasteries.Where(cm => cm.ChampionId == championId)
                .OrderByDescending(cm => cm.Points)
                .Include(cm => cm.Summoner.User)
                .GroupBy(cm => cm.Summoner.UserId)
                .Select(group  => new LeaderboardEntryViewModel
                {
                    Name = group.Any() ? group.FirstOrDefault().Summoner.User.Name : null,
                    TotalPoints = group.Select(cm => cm.Points).Sum()
                })
                .Where(cm => cm.TotalPoints >= minPoints)
                .OrderByDescending(entry => entry.TotalPoints);
            var entries = await (count < 0 ? entriesQuery.ToListAsync() : entriesQuery.Take(count).ToListAsync());

            return new LeaderboardViewModel
            {
                ChampionId = championId,
                Entries = entries
            };
        }
    }
}
