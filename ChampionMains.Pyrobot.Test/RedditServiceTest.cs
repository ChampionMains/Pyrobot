using System.Threading.Tasks;
using ChampionMains.Pyrobot.Services;
using ChampionMains.Pyrobot.Services.Reddit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChampionMains.Pyrobot.Test
{
    [TestClass]
    public class RedditServiceTest
    {
        private readonly RedditService _redditService = TestSimpleInjector.GetInstance<RedditService>();

        [Ignore]
        [TestMethod]
        public async Task TestGetMods()
        {
            await _redditService.GetModSubredditsAsync(new []{ "config", "flair" });
        }
    }
}
