using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Util;
using Reddit.Things;
using Subreddit = ChampionMains.Pyrobot.Data.Models.Subreddit;
using User = ChampionMains.Pyrobot.Data.Models.User;

namespace ChampionMains.Pyrobot.Services
{
    public class FlairService
    {
        private readonly UnitOfWork _context;
        private readonly TimeSpan _flairUpdateMaxStaleTime;

        public FlairService(UnitOfWork context, ApplicationConfiguration config)
        {
            _context = context;
            _flairUpdateMaxStaleTime = config.FlairUpdate;
        }

        public async Task<ICollection<Subreddit>> GetSubredditsForFlairUpdateAsync(int numSubreddits, CancellationToken token)
        {
            return await _context.Subreddits
                    .Where(s => !s.MissingMod)
                    .OrderByDescending(s => s.LastBulkUpdate == null)
                    .ThenBy(s => s.LastBulkUpdate)
                    .Take(numSubreddits)
                    .Include(s => s.SubredditUserFlairs.Select(f => f.User))
                    .ToListAsync(token);
        }

//        public async Task<ICollection<SubredditUserFlair>> GetFlairsForUpdateAsync(CancellationToken token)
//        {
//            var staleAfter = DateTimeOffset.Now - _flairUpdateMaxStaleTime;
//            return await _context.SubredditUserFlairs
//                    .Include(f => f.Subreddit)
//                    .Where(f => !f.Subreddit.MissingMod && (f.LastUpdate == null || f.LastUpdate < staleAfter))
//                    .OrderBy(f => f.LastUpdate)
//                    .ThenBy(f => f.Id)
//                    .Take(1000) // Limit to 1,000 arbitrary. TODO.
//                    .OrderBy(f => f.SubredditId)
//                    .Include(f => f.User)
//                    .ToListAsync(token);
//        }


        public async Task<ICollection<SubredditUserFlair>> GetSubredditUserFlairs(User user)
        {
            return await _context.SubredditUserFlairs.Where(f => f.User.Id == user.Id).ToListAsync();
        }

        public Subreddit GetSubreddit(int id)
        {
            return _context.Subreddits.Find(id);
        }

        public async Task<bool> UpdateSubredditUserFlair(int userId, int subredditId, bool rankEnabled,
            bool championMasteryEnabled, bool prestigeEnabled, bool championMasteryTextEnabled, string flairText)
        {
            var subredditUserFlair = _context.SubredditUserFlairs.FirstOrDefault(u => u.UserId == userId && u.SubredditId == subredditId);

            // if nothing is enabled, remove the flair
            if (!rankEnabled && !championMasteryEnabled && !prestigeEnabled &&
                !championMasteryTextEnabled && string.IsNullOrWhiteSpace(flairText))
            {
                if (subredditUserFlair != null)
                    _context.SubredditUserFlairs.Remove(subredditUserFlair);
                else
                    return true;
            }
            else
            {
                if (subredditUserFlair == null)
                {
                    var subreddit = _context.Subreddits.Find(subredditId);
                    if (subreddit == null)
                        return false;

                    subredditUserFlair = new SubredditUserFlair
                    {
                        UserId = userId,
                        SubredditId = subredditId
                    };
                    _context.SubredditUserFlairs.Add(subredditUserFlair);
                }

                subredditUserFlair.RankEnabled = rankEnabled;
                subredditUserFlair.ChampionMasteryEnabled = championMasteryEnabled;
                subredditUserFlair.PrestigeEnabled = prestigeEnabled;
                subredditUserFlair.ChampionMasteryTextEnabled = championMasteryTextEnabled;
                subredditUserFlair.FlairText = flairText;

                subredditUserFlair.LastUpdate = DateTimeOffset.Now;
            }

            await _context.SaveChangesAsync();
            return true; //await _context.SaveChangesAsync() > 0; // will always be true due to LastUpdate field
        }

        public const string RankPrefix = "rank-";
        public const string MasteryPrefix = "mastery-";
        public const string PrestigePrefix = "prestige-";
        public const string MasteryTextClass = "masteryText";

        public FlairListResult GenerateFlair(string username, ICollection<Summoner> summoners, Subreddit subreddit, bool userRankEnabled,
            bool userMasteryEnabled, bool userPrestigeEnabled, bool userMasteryTextEnabled, string text, string oldCss)
        {
            var championId = subreddit.ChampionId;

            var rankEnabled = subreddit.RankEnabled && userRankEnabled;
            var masteryEnabled = subreddit.ChampionMasteryEnabled && userMasteryEnabled;
            var prestigeEnabled = subreddit.PrestigeEnabled && userPrestigeEnabled;
            var masteryTextEnabled = subreddit.ChampionMasteryTextEnabled && userMasteryTextEnabled;

            var classes = new List<string>();
            // re-add the old css classes, ignoring ones generated by the bot
            if (oldCss != null)
                classes.AddRange(oldCss.Split().Where(c => !string.IsNullOrWhiteSpace(c) && !c.StartsWith(RankPrefix)
                && !c.StartsWith(MasteryPrefix) && !c.StartsWith(PrestigePrefix) && !MasteryTextClass.Equals(c)));

            if (rankEnabled)
            {
                var tier = (Tier) summoners.Select(s => s.Rank.Tier).DefaultIfEmpty().Max();
                classes.Add((RankPrefix + tier).ToLower());
            }

            if (masteryEnabled)
            {
                var masteryLevel = summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Level ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a > b ? a : b);

                if (masteryLevel > subreddit.MinimumChampionMasteryLevel)
                    classes.Add(MasteryPrefix + masteryLevel);
            }

            if (prestigeEnabled || masteryTextEnabled)
            {
                var masteryPoints = summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Points ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a + b);

                if (prestigeEnabled)
                {
                    var prestige = RankUtil.GetPrestigeLevel(masteryPoints) / 1000;

                    if (prestige > 0)
                        classes.Add(PrestigePrefix + prestige);
                }

                if (masteryTextEnabled)
                {
                    text = FlairUtil.PrependFlairTextLeadingMastery(text, masteryPoints);
                    classes.Add(MasteryTextClass);
                }
            }

            return new FlairListResult
            {
                User = username,
                FlairCssClass = string.Join(" ", classes),
                FlairText = text
            };
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
