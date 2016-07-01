using System.Collections.Generic;
using System.Web.Http;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    public class PeriodicUpdateApiController : ApiController
    {
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly WebJobService _webJob;

        public PeriodicUpdateApiController(UserService users, SummonerService summoners, WebJobService webJob)
        {
            _users = users;
            _summoners = summoners;
            _webJob = webJob;
        }

        [HttpPost]
        [Route("profile/api/asdf")]
        public IEnumerable<int> GetPrestiges()
        {
            //TODO
            return RankUtil.PrestigeLevels;
        }
    }
}
