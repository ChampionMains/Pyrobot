using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;

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

        public async Task<ICollection<SubredditUserFlair>> GetFlairsForUpdateAsync()
        {
            var staleAfter = DateTimeOffset.Now - _flairUpdateMaxStaleTime;
            return await _context.SubredditUserFlairs.Where(s => s.LastUpdate == null || s.LastUpdate < staleAfter)
                    //.Include(x => x.User.Summoners.Select(s => s.ChampionMasteries))
                    //.Include(x => x.User.Summoners.Select(s => s.Rank))
                    .Include(x => x.User)
                    .Include(x => x.Subreddit).ToListAsync();
        }


        public async Task<ICollection<SubredditUserFlair>> GetSubredditUserFlairs(User user)
        {
            return await _context.SubredditUserFlairs.Where(f => f.User.Id == user.Id).ToListAsync();
        }

        public async Task<bool> UpdateSubredditUserFlair(int userId, int subredditId, bool rankEnabled,
            bool championMasteryEnaabled, bool prestigeEnabled, string flairText)
        {
            var subredditUserFlair = _context.SubredditUserFlairs.FirstOrDefault(u => u.UserId == userId && u.SubredditId == subredditId);

            // if nothing is enabled, remove the flair
            if (!rankEnabled && !championMasteryEnaabled && !prestigeEnabled && string.IsNullOrWhiteSpace(flairText))
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
                subredditUserFlair.ChampionMasteryEnabled = championMasteryEnaabled;
                subredditUserFlair.PrestigeEnabled = prestigeEnabled;
                subredditUserFlair.FlairText = flairText;

                subredditUserFlair.LastUpdate = DateTimeOffset.Now;
            }

            await _context.SaveChangesAsync();
            return true; //await _context.SaveChangesAsync() > 0; // will always be true due to LastUpdate field
        }

        public string GenerateFlairCSS(int userId, short championId, bool rankEnabled, bool masteryEnabled, bool prestigeEnabled, string oldCss)
        {
            const string rankPrefix = "rank-";
            const string masteryPrefix = "mastery-";
            const string prestigePrefix = "prestige-";

            var summonersQuery = _context.Summoners.Where(s => s.UserId == userId);
            if (rankEnabled)
                summonersQuery = summonersQuery.Include(s => s.Rank);
            if (masteryEnabled || prestigeEnabled)
                summonersQuery = summonersQuery.Include(s => s.ChampionMasteries);
            var summoners = summonersQuery.ToList();

            var classes = new List<string>();
            if (oldCss != null)
                classes.AddRange(oldCss.Split().Where(c => !string.IsNullOrWhiteSpace(c)
                    && !c.StartsWith(rankPrefix) && !c.StartsWith(masteryPrefix) && !c.StartsWith(prestigePrefix)));

            if (rankEnabled)
            {
                var tier = (Tier) summoners.Select(s => s.Rank.Tier).DefaultIfEmpty().Max();
                classes.Add((rankPrefix + tier).ToLower());
            }

            if (masteryEnabled || prestigeEnabled)
            {
                var masteryLevel = summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Level ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a > b ? a : b);

                if (masteryEnabled && masteryLevel > 0)
                    classes.Add(masteryPrefix + masteryLevel);

                var masteryPoints = summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Points ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a + b);

                var prestige = RankUtil.GetPrestigeLevel(masteryPoints) / 1000;

                if (prestige > 0)
                    classes.Add(prestigePrefix + prestige);
            }

            return string.Join(" ", classes);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}