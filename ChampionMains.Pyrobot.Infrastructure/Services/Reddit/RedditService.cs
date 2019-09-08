using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Util;
using Reddit.Inputs;
using Reddit.Inputs.Flair;
using Reddit.Inputs.PrivateMessages;
using Reddit.Things;

namespace ChampionMains.Pyrobot.Services.Reddit
{
    public class RedditService
    {
        private const int MaxFlairUpdateSize = 100;
        private const int MaxLimitSize = 100;

        private readonly RedditApiProvider _redditApiProvider;

        public RedditService(RedditApiProvider redditApiProvider)
        {
            _redditApiProvider = redditApiProvider;
        }

        public string GetBotUsername()
        {
            return _redditApiProvider.GetBotUsername();
        }

        public async Task SendMessageAsync(string toUserName, string subject, string body)
        {
            const int maxSubjectLength = 100;

            if (string.IsNullOrEmpty(toUserName)) throw new ArgumentException("toUserName is required.");
            if (string.IsNullOrEmpty(subject)) throw new ArgumentException("subject is required.");
            if (string.IsNullOrEmpty(body)) throw new ArgumentException("body is required.");
            if (subject.Length > maxSubjectLength) throw new ArgumentException("subject must not be longer than 100 characters.");
            
            var reddit = await _redditApiProvider.GetRedditApi();
            await reddit.Account.Messages.ComposeAsync(new PrivateMessagesComposeInput(to: toUserName, subject: subject, text: body));

            // for some reason, the /api/compose endpoint uses a POST request but wants its
            // parameters in the query string. TODO?
        }

        public async Task<FlairListResult> GetFlairAsync(string subreddit, string name)
        {

            var flair = (await GetFlairsAsync(subreddit, name)).FirstOrDefault();
            // If flair doesn't exist, UserFlairParameter will parse,
            // but flair_css_class and flair_text will be null.
            // Flair will never be null
            //
            // Note this only applies then the "name" parameter is set
            return flair?.FlairCssClass == null ? null : flair;
        }

        public async Task<IList<FlairListResult>> GetFlairsAsync(string subreddit, string name = "")
        {
            var reddit = await _redditApiProvider.GetRedditApi();

            var flairModel = reddit.Models.Flair;
            var input = new FlairNameListingInput(name, limit: MaxLimitSize);
            var flairs = new List<FlairListResult>();

            do
            {
                // TODO Add FlairListAsync.
                var output = await flairModel.SendRequestAsync<FlairListResultContainer>(
                    flairModel.Sr(subreddit) + "api/flairlist", input);
                input.after = output.Next;

                flairs.AddRange(output.Users);

            } while (!string.IsNullOrWhiteSpace(input.after));

            return flairs;
        }

        public async Task SetFlairAsync(string subreddit, string name, string text = null, string classes = null)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var flairController = reddit.Subreddit(subreddit).Flairs;

            await flairController.CreateUserFlairAsync(
                new FlairCreateInput(text ?? "", name: name, cssClass: classes));
        }

        /// <summary>
        /// Sets flairs in bulk. Returns the number of flairs that succeeded. Throws an exception if no flairs succeeded (rather than returning zero).
        /// </summary>
        public async Task<int> SetFlairsAsync(string subreddit, ICollection<FlairListResult> flairs)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var flairController = reddit.Subreddit(subreddit).Flairs;

            var errorResultTasks = flairs.Select((flair, i) => new {flair, g = i / MaxFlairUpdateSize}).GroupBy(x => x.g, x => x.flair)
                .Select(async groupedFlairs =>
                {
                    var csv = ResolveBulkFlairParameter(groupedFlairs);
                    try
                    {
                        var result = await flairController.FlairCSVAsync(csv);
                        return result.Count(r => r.Ok);
                    }
                    catch (Exception e)
                    {
                        var numBatches = (int) Math.Ceiling(flairs.Count * 1.0 / MaxFlairUpdateSize);
                        // ReSharper disable once LocalizableElement
                        Console.WriteLine($"Flair update for subreddit {subreddit} batch {groupedFlairs.Key + 1}/{numBatches} FAILED with exception:\n{e}");
                        throw;
                    }
                })
                .ToList();

            var oks = 0;
            var exceptions = new List<Exception>();
            foreach (var errorResultTask in errorResultTasks)
            {
                try
                {
                    oks += await errorResultTask;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (oks <= 0)
                throw new AggregateException(exceptions);
            return oks;
        }

        public async Task<HashSet<string>> GetModSubredditsAsync(string user, string[] permissions)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var subredditsModel = reddit.Models.Subreddits;
            var input = new CategorizedSrListingInput(limit: 100);
            // Make result set case-insensitive.
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            do
            {
                // TODO upstream add MineAsync:
                // var output = subredditsModel.MineAsync("moderator", input);
                var output = await subredditsModel.SendRequestAsync<SubredditContainer>("subreddits/mine/moderator", input);
                input.after = output.Data.after;

                foreach (var subreddit in output.Data.Children)
                {
                    // Only add if permissions are satisfied.
                    if (null == subreddit.Data?.ModPermissions)
                        continue;
                    if (subreddit.Data.ModPermissions.Contains("all") || subreddit.Data.ModPermissions.ContainsAll(permissions))
                        result.Add(subreddit.Data.DisplayName);
                }
            } while (!string.IsNullOrWhiteSpace(input.after));

            return result;
        }

        private static string ResolveBulkFlairParameter(IEnumerable<FlairListResult> flairs)
        {
            return string.Join("\n", flairs.Select(f => EscapeCsv(new[] {f.User, f.FlairText ?? "", f.FlairCssClass ?? ""})));
        }

        private static string EscapeCsv(IEnumerable<string> values)
        {
            const string quote = "\"";
            const string quoteEscaped = "\"\"";
            char[] needsQuotes = { ',', '"', '\n' };

            return string.Join(",", values.Select(s =>
            {
                if (s.Contains(quote))
                    s = s.Replace(quote, quoteEscaped);

                if (s.IndexOfAny(needsQuotes) >= 0)
                    s = quote + s + quote;

                return s;
            }));
        }
    }
}
