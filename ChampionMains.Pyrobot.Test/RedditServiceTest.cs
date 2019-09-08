using System.IO;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Services.Reddit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChampionMains.Pyrobot.Test
{
    [TestClass]
    public class RedditServiceTest
    {
        private readonly RedditService _redditService = TestSimpleInjector.GetInstance<RedditService>();
        private readonly EmojiService _emojiService = TestSimpleInjector.GetInstance<EmojiService>();

        [Ignore]
        [TestMethod]
        public async Task TestGetMods()
        {
            await _redditService.GetModSubredditsAsync(new []{ "config", "flair" });
        }

        private const string ImageFile = @"cm-icon.png";

        [TestMethod]
        [DeploymentItem(ImageFile)]
        public async Task TestUploadImage()
        {
            var fileStream = new FileStream(ImageFile, FileMode.Open);
            var file = new MemoryFile(fileStream, "image/png", ImageFile);
            await _emojiService.UploadEmoji("kledmains", "champmains", file);
        }
    }
}
