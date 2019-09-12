using System;
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
        public async Task TestGetModSubreddits()
        {
            await _redditService.GetBotModeratedSubredditsAsync(new []{ "config", "flair" });
        }

        [TestMethod]
        public async Task TestGetSubredditMods()
        {
            var mods = await _redditService.GetSubredditModsAsync("kledmains", new[] {"config", "flair"});
            Console.WriteLine(string.Join(",", mods));
            Assert.IsTrue(mods.Contains("lugNUTSk"));
        }

        private const string ImageFile = @"cm-icon.png";
        [Ignore]
        [TestMethod]
        [DeploymentItem(ImageFile)]
        public async Task TestUploadImage()
        {
            var fileStream = new FileStream(ImageFile, FileMode.Open);
            var file = new MemoryFile(fileStream, "image/png", ImageFile);
            await _emojiService.UploadEmoji("kledmains", "cm", file, true, false, false);

            var allEmoji = await _emojiService.GetAllEmoji("kledmains");
        }
    }
}
