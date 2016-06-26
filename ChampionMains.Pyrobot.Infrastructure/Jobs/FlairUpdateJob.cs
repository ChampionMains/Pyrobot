using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.Jobs
{
    public class FlairUpdateJob
    {
        private readonly FlairService _flairs;
        private readonly UserService _users;

        public FlairUpdateJob(FlairService flairs, UserService users)
        {
            _flairs = flairs;
            _users = users;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.FlairUpdate)] int userId)
        {
            var user = await _users.FindAsync(userId);
            await _flairs.SetUpdateFlagAsync(new[] {user});
        }
    }
}