using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ChampionMains.Pyrobot.Attributes;
using ChampionMains.Pyrobot.Services;

namespace ChampionMains.Pyrobot.Controllers
{
    public class PeriodicUpdateApiController : ApiController
    {
        private readonly FlairService _flair;
        private readonly SummonerService _summoners;
        private readonly UserService _users;
        private readonly WebJobService _webJob;

        public PeriodicUpdateApiController(UserService users, SummonerService summoners, FlairService flair, WebJobService webJob)
        {
            _users = users;
            _summoners = summoners;
            _flair = flair;
            _webJob = webJob;
        }

        [HttpPost]
        [Route("profile/api/update")]
        public IEnumerable<int> GetPrestiges()
        {
            return RankUtil.PrestigeLevels;
        }
    }
}
