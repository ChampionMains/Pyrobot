using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Services.Reddit;

namespace ChampionMains.Pyrobot.Services
{
    public class RedditService : IRedditService, IRedditMessengerService
    {
        private const string BaseUri = "https://oauth.reddit.com";
        private readonly RedditWebRequester _requester;

        public RedditService(RedditWebRequester requester)
        {
            _requester = requester;
        }

        public async Task<ICollection<string>> GetSubRedditsAsync(SubRedditKind kind)
        {
            var uri = $"{BaseUri}/subreddits/mine/{kind.ToString().ToLowerInvariant()}";
            var results = new List<string>();
            var after = (string) null;

            while (true)
            {
                var data = after == null
                    ? null
                    : new[]
                    {
                        new KeyValuePair<string, string>("after", after), 
                    };
                var listing = await _requester.GetAsync(uri, data);
                var content = listing["data"];
                after = (string) content["after"];
                results.AddRange(from item in content["children"]
                                 where (string) item["kind"] == "t5"
                                 select (string) item["data"]["display_name"]);

                if (after == null) break;
            }
            return results;
        }

        public async Task<bool> SendMessageAsync(string toUserName, string subject, string body)
        {
            const int maxSubjectLength = 100;

            if (string.IsNullOrEmpty(toUserName)) throw new ArgumentException("toUserName is required.");
            if (string.IsNullOrEmpty(subject)) throw new ArgumentException("subject is required.");
            if (string.IsNullOrEmpty(body)) throw new ArgumentException("body is required.");
            if (subject.Length > maxSubjectLength) throw new ArgumentException("subject must not be longer than 100 characters.");

            // for some reason, the /api/compose endpoint uses a POST request but wants its
            // parameters in the query string.
            var parameters = new[]
            {
                new { key = "to", value = toUserName },
                new { key = "subject", value = subject },
                new { key = "text", value = body }
            };

            var pairs = from p in parameters
                        select $"{Uri.EscapeDataString(p.key)}={Uri.EscapeDataString(p.value)}";

            var uri = $"{BaseUri}/api/compose?{string.Join("&", pairs)}";
            Trace.WriteLine("POST " + uri);
            var result = await _requester.PostAsync(uri);

            return true;
        }

        public async Task<bool> SetUserFlairAsync(string subreddit, string name, string text, string css = null)
        {
            var uri = $"{BaseUri}/r/{subreddit}/api/flair";
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("text", "text")
            };
            if (css != null)
            {
                data.Add(new KeyValuePair<string, string>("css_class", css));
            }

            var result = await _requester.PostAsync(uri, data);
            return !result["errors"].Any();
        }

        public async Task<bool> SetUserFlairsAsync(string subreddit, ICollection<UserFlairParameter> flairs)
        {
            if (flairs.Count > 100)
            {
                throw new InvalidOperationException(
                    "The API does not support assigning more than 100 flairs in one operation");
            }
            var uri = $"{BaseUri}/r/{subreddit}/api/flaircsv";
            var data = new[]
            {
                new KeyValuePair<string, string>("flair_csv", ResolveBulkFlairParameter(flairs))
            };
            var result = await _requester.PostAsync(uri, data);
            return !result.Any(token => token["errors"].Any());
        }

        private static string ResolveBulkFlairParameter(IEnumerable<UserFlairParameter> flairs)
        {
            return string.Join("\n", flairs.Select(f => $"{f.Name},{f.Text ?? ""},{f.CssClass ?? ""}"));
        }
    }
}
