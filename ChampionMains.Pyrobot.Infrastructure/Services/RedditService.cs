using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Services.Reddit;

namespace ChampionMains.Pyrobot.Services
{
    public class RedditService
    {
        private const int MaxFlairGetSize = 1000;
        private const int MaxFlairUpdateSize = 100;

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
            var nextToken = "";

            do
            {
                var listing = await _requester.GetAsync(uri, new[]
                {
                    new KeyValuePair<string, string>("after", nextToken)
                });

                var content = listing["data"];
                nextToken = (string) content["after"];
                results.AddRange(from item in content["children"]
                    where (string) item["kind"] == "t5"
                    select (string) item["data"]["display_name"]);
                
            } while (nextToken != null);

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

        public async Task<UserFlairParameter> GetFlairAsync(string subreddit, string name)
        {
            var flair = (await GetFlairsAsync(subreddit, name)).FirstOrDefault();
            // If flair doesn't exist, UserFlairParameter will parse,
            // but flair_css_class and flair_text will be null.
            // Flair should never be null
            return flair?.CssClass == null ? null : flair;
        }

        public async Task<ICollection<UserFlairParameter>> GetFlairsAsync(string subreddit, string name = "")
        {
            var flairs = new List<UserFlairParameter>();
            var nextToken = "";

            var uri = $"{BaseUri}/r/{subreddit}/api/flairlist";

            do
            {
                var data = new[]
                {
                    new KeyValuePair<string, string>("limit", MaxFlairGetSize.ToString()),
                    new KeyValuePair<string, string>("after", nextToken),
                    new KeyValuePair<string, string>("name", name)
                };
                var result = await _requester.GetAsync(uri, data);
                flairs.AddRange(result["users"].ToObject<ICollection<UserFlairParameter>>());
                nextToken = result["next"]?.ToString();

            } while (nextToken != null);

            return flairs;
        }

        public async Task<bool> SetFlairAsync(string subreddit, string name, string text = null, string classes = null)
        {
            var uri = $"{BaseUri}/r/{subreddit}/api/flair";
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("name", name)
            };
            if (text != null)
                data.Add(new KeyValuePair<string, string>("text", text));
            if (classes != null)
                data.Add(new KeyValuePair<string, string>("css_class", classes));

            var result = await _requester.PostAsync(uri, data);

            return !result["json"]["errors"].Any();
        }

        public async Task<bool> SetFlairsAsync(string subreddit, ICollection<UserFlairParameter> flairs)
        {
            var errors = flairs.Select((flair, i) => new {flair, g = i / MaxFlairUpdateSize}).GroupBy(x => x.g)
                .Select(async group =>
                {
                    var uri = $"{BaseUri}/r/{subreddit}/api/flaircsv";
                    var data = new[]
                    {
                        new KeyValuePair<string, string>("flair_csv", ResolveBulkFlairParameter(flairs))
                    };
                    var result = await _requester.PostAsync(uri, data);
                    return result.Any(token => token["errors"].Any());
                }).ToList();
            await Task.WhenAll(errors);
            return errors.Any(x => x.Result);
        }

        private static string ResolveBulkFlairParameter(IEnumerable<UserFlairParameter> flairs)
        {
            return string.Join("\n", flairs.Select(f => $"{f.Name},{f.Text ?? ""},{f.CssClass ?? ""}"));
        }
    }
}
