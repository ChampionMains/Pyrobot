using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Reddit.Inputs.Emoji;
using Reddit.Things;
using RestSharp;

namespace ChampionMains.Pyrobot.Services.Reddit
{
    public class EmojiService
    {
        private readonly RedditApiProvider _redditApiProvider;

        public EmojiService(RedditApiProvider redditApiProvider)
        {
            _redditApiProvider = redditApiProvider;
        }

        public async Task<List<Snoomoji>> GetEmoji(string subredditName)
        {
            var reddit = await _redditApiProvider.GetRedditApi();

            var emojiContainer = JsonConvert.DeserializeObject<SnoomojiContainer>(
                await reddit.Models.Emoji.ExecuteRequestAsync("api/v1/" + subredditName + "/emojis/all"));

            return emojiContainer.SubredditEmojis?.Values.ToList();
        }

        public async Task UploadEmoji(string subredditName, string emojiName, HttpPostedFileBase emojiImageFile)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var emoji = reddit.Models.Emoji;

            var imageUploadInput = new ImageUploadInput(emojiImageFile.FileName, emojiImageFile.ContentType);
            var lease = await emoji.AcquireLeaseAsync(subredditName, imageUploadInput);
            await emoji.UploadLeaseImageAsync(lease, emojiImageFile.InputStream, imageUploadInput);

            Console.WriteLine(@"TODO");
            // TODO keep going.
        }
    }
}
