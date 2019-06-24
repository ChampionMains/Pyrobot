using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Services.Reddit;
using Reddit.Inputs.Flair;
using Reddit.Inputs.PrivateMessages;
using Reddit.Things;
using Subreddit = Reddit.Controllers.Subreddit;

namespace ChampionMains.Pyrobot.Services
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

        public async Task<ICollection<string>> GetSubredditsAsync(SubredditKind kind)
        {
            var reddit = await _redditApiProvider.GetRedditApi();

            var subreddits = new List<Subreddit>();
            var moreSubreddits = reddit.Account.MyModeratorSubreddits(limit: MaxLimitSize);
            while (moreSubreddits.Any())
            {
                subreddits.AddRange(moreSubreddits);
                moreSubreddits = reddit.Account.MyModeratorSubreddits(limit: MaxLimitSize, count: subreddits.Count);
            }

            return subreddits.Select(s => s.Name).ToList();
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

            var flairController = reddit.Subreddit(subreddit).Flairs;
            var args = new FlairNameListingInput(name, limit: MaxLimitSize);
            var flairs = new List<FlairListResult>();
            do
            {
                flairs.AddRange(flairController.GetFlairList(args));
            } while (!string.IsNullOrWhiteSpace(flairController.FlairListNext));

            return flairs;
        }

        public async Task SetFlairAsync(string subreddit, string name, string text = null, string classes = null)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var flairController = reddit.Subreddit(subreddit).Flairs;

            await flairController.CreateUserFlairAsync(
                new FlairCreateInput(text ?? "", name: name, cssClass: classes));
        }

        public async Task SetFlairsAsync(string subreddit, ICollection<FlairListResult> flairs)
        {
            var reddit = await _redditApiProvider.GetRedditApi();
            var flairController = reddit.Subreddit(subreddit).Flairs;

            var errorResultTasks = flairs.Select((flair, i) => new {flair, g = i / MaxFlairUpdateSize}).GroupBy(x => x.g, x => x.flair)
                .Select(groupedFlairs => flairController.FlairCSVAsync(ResolveBulkFlairParameter(groupedFlairs)))
                .ToList();
            await Task.WhenAll(errorResultTasks);
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
