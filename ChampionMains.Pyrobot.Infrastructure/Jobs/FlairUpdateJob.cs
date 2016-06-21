using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs.Jobs
{
    public class FlairUpdateJob
    {
        private readonly IFlairService _flairs;
        private readonly IUserService _users;

        public FlairUpdateJob(IFlairService flairs, IUserService users)
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