using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Models;
using ChampionMains.Pyrobot.Services;
using WebApi.OutputCache.V2;

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
            await _webJob.QueueBulkUpdate(DateTime.UtcNow.ToString("s"));
            return true;
        }

        [HttpPost]
        [Route("trigger-subreddit-css-update")]
        public async Task<bool> TriggerSubredditCssUpdate()
        {
            await _webJob.QueueSubredditCssUpdate(DateTime.UtcNow.ToString("s"));
            return true;
        }

        [Route("api/user-summoners")]
        [CacheOutput(ServerTimeSpan = 0 /*60 * 5*/, ClientTimeSpan = 50 * 5)]
        public async Task<List<SummonerInfoViewModel>> GetUserSummoners(string username)
        {
            var user = await _users.FindAsync(username);
            if (user == null)
                throw new HttpResponseException(HttpStatusCode.NoContent);

            return _unitOfWork.Summoners.Where(s => s.UserId == user.Id)
                .Select(s => new SummonerInfoViewModel
                {
                    Name = s.Name,
                    Region = s.Region
                }).ToList();
        }

        [Route("api/leaderboard")]
        [CacheOutput(ServerTimeSpan = 0 /*60 * 5*/, ClientTimeSpan = 50 * 5)]
        public async Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken,
            int championId, int count = 10, int minPoints = 0)
        {
            // Approximation by ignoring summoners with little mastery of the champion.
            // Greatly reduces the number of SummonerChampionMasteries query needs to look at since
            // most people have low mastery on many champions.
            const int perSummonerThreshold = 1000;

            if (count <= 0 || 500 < count || minPoints < 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            
            // Be careful of SQL injection!
            // All these fields are non-nullable ints so we are safe.
            var entries = await _unitOfWork.Database.SqlQuery<LeaderboardEntryViewModel>($@"
                SELECT [User].[Name] AS Name, TotalPoints
                FROM [User]
                INNER JOIN (
                    SELECT TOP {count}
                    [User].Id AS UserId, SUM(SummonerChampionMastery.Points) AS TotalPoints
                    FROM SummonerChampionMastery
                    INNER JOIN Summoner ON SummonerChampionMastery.SummonerId = Summoner.Id
                    INNER JOIN [User] ON Summoner.UserId = [User].Id
                    WHERE SummonerChampionMastery.ChampionId = {championId}
                    AND SummonerChampionMastery.Points >= {perSummonerThreshold}
                    GROUP BY [User].Id
		                HAVING SUM(SummonerChampionMastery.Points) >= {minPoints}
                    ORDER BY TotalPoints DESC
                ) AS PointsTable ON [User].Id = PointsTable.UserId
                ORDER BY TotalPoints DESC").ToListAsync(cancellationToken);

            return new LeaderboardViewModel
            {
                ChampionId = championId,
                Entries = entries
            };
        }
    }
}
