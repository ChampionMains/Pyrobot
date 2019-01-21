using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Services;
using Microsoft.Azure.WebJobs;

namespace ChampionMains.Pyrobot.WebJob.Jobs
{
    /// <summary>
    /// Updates stored subreddit CSS.
    /// </summary>
    public class SubredditCssUpdateJob
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly SubredditService _subredditService;

        private readonly TimeSpan _timeout; // TODO?

        public SubredditCssUpdateJob(SubredditService subredditService)
        {
            _subredditService = subredditService;
        }

        public async Task Execute([QueueTrigger(WebJobQueue.SubredditCssUpdate)] string args)
        {
            if (!await Lock.WaitAsync(1000))
                throw new InvalidOperationException("Lock is not available. Task may already be running.");

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(60 * 1000);

                try
                {
                    await UpdateSubredditCss(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    throw new TimeoutException($"{nameof(SubredditCssUpdateJob)} timed out ({_timeout})", e);
                }
            }
            finally
            {
                Lock.Release();
            }
        }

        private async Task UpdateSubredditCss(CancellationToken token)
        {
            var tasks = (await _subredditService.GetSubreddits()).Select(async subreddit =>
            {
                var subredditName = subreddit.Name;
                var request = WebRequest.Create($"https://old.reddit.com/r/{subredditName}/stylesheet.css");

                var response = await request.GetResponseAsync();

                token.ThrowIfCancellationRequested();

                var data = response.GetResponseStream();
                using (var reader = new StreamReader(data))
                {
                    var css = await reader.ReadToEndAsync();
                    var sb = new StringBuilder();

                    token.ThrowIfCancellationRequested();

                    var matches = Regex.Matches(css, // Cancer. Magic cancer.
                        @"(?<=\}|^)([^\}\{]*(?<=,|\}|^)\.flair\b[^\}\{]*)((?'brace'\{.*?)+(?'-brace'\}.*?)+)+(?(brace)(?!))");
                    foreach (Match match in matches)
                    {
                        var oldSelectors = match.Groups[1].Value; // ".flair-rank-challenger,.flair-rank-diamond,.flair-rank-master"
                        var styles = match.Groups[2].Value; // "{outline:#000 solid 1px}"

                        var selectors = oldSelectors.Split(',');
                        for (var i = 0; i < selectors.Length; i++)
                        {
                            // ".subreddit-zyramains>.flair-rank-challenger" comma separated
                            var selector = selectors[i];
                            if (i != 0)
                                sb.Append(',');
                            sb.Append(".subreddit-").Append(subredditName).Append('>').Append(selector);
                        }
                        sb.Append(styles);
                    }
                    subreddit.FlairCss = sb.ToString();
                }
            }).ToList();
            await Task.WhenAll(tasks);
            var updates = await _subredditService.SaveChangesAsync();
            Console.Out.WriteLine($"Updating subreddit css {(token.IsCancellationRequested ? "interrupted" : "complete")}, {updates} rows affected.");
        }
    }
}
