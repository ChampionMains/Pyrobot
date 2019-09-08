using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public async Task<List<Snoomoji>> GetEmoji(string subredditName)
        {
            var reddit = await _redditApiProvider.GetRedditApi();

            var emojiContainer = JsonConvert.DeserializeObject<SnoomojiContainer>(
                await reddit.Models.Emoji.ExecuteRequestAsync("api/v1/" + subredditName + "/emojis/all"));

            return emojiContainer.SubredditEmojis?.Values.ToList();
        }
    }
}
