using System.Threading.Tasks;
using ChampionMains.Pyrobot.Services;

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

        public void Execute(int userId)
        {
            ExecuteInternal(userId).Wait();
        }

        private async Task ExecuteInternal(int userId)
        {
            var user = await _users.FindAsync(userId);
            await _flairs.SetUpdateFlagAsync(new[] {user});
        }
    }
}