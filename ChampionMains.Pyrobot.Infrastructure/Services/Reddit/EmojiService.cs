using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Reddit.Inputs.Emoji;
using Reddit.Things;

namespace ChampionMains.Pyrobot.Services.Reddit
{
    public class EmojiService
    {
        private readonly RedditApiProvider _redditApiProvider;

        public EmojiService(RedditApiProvider redditApiProvider)
        {
            _redditApiProvider = redditApiProvider;
        }

        public async Task<List<Snoomoji>> GetAllEmoji(string subredditName)
        {
            var reddit = await _redditApiProvider.GetRedditApi();

            var emojiContainer = JsonConvert.DeserializeObject<SnoomojiContainer>(
                await reddit.Models.Emoji.ExecuteRequestAsync("api/v1/" + subredditName + "/emojis/all"));

            return emojiContainer.SubredditEmojis?.Values.ToList();
        }

        public async Task UploadEmoji(string subredditName, string emojiName, HttpPostedFileBase emojiImageFile,
            bool modFlairOnly, bool postFlairAllowed, bool userFlairAllowed)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var emoji = reddit.Models.Emoji;

            var imageUploadInput = new ImageUploadInput(emojiImageFile.FileName, emojiImageFile.ContentType);
            var lease = await emoji.AcquireLeaseAsync(subredditName, imageUploadInput);
            var s3Resposne = await emoji.UploadLeaseImageAsync(lease, emojiImageFile.InputStream, imageUploadInput);
            var emojiAddInput = new EmojiAddInput(emojiName, s3Resposne.Key, modFlairOnly, postFlairAllowed, userFlairAllowed);
            var anyErrors = emoji.Add(subredditName, emojiAddInput); // TODO async.
            // TODO check anyErrors?.
        }
    }
}
