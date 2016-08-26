﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using ChampionMains.Pyrobot.Reddit;
using ChampionMains.Pyrobot.Util;

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

        public UserFlairParameter GenerateFlair(User user, Subreddit subreddit, bool userRankEnabled, bool userMasteryEnabled,
            bool userPrestigeEnabled, bool userMasteryTextEnabled, string text, string oldCss)
        {
            const string rankPrefix = "rank-";
            const string masteryPrefix = "mastery-";
            const string prestigePrefix = "prestige-";
            const string masteryTextClass = "masteryText";

            var championId = subreddit.ChampionId;

            var rankEnabled = subreddit.RankEnabled && userRankEnabled;
            var masteryEnabled = subreddit.ChampionMasteryEnabled && userMasteryEnabled;
            var prestigeEnabled = subreddit.PrestigeEnabled && userPrestigeEnabled;
            var masteryTextEnabled = subreddit.ChampionMasteryTextEnabled && userMasteryTextEnabled;

            // make sure text is non-null
            if (text == null)
                text = "";

            var summonersQuery = _context.Summoners.Where(s => s.UserId == user.Id);
            if (rankEnabled)
                summonersQuery = summonersQuery.Include(s => s.Rank);
            if (masteryEnabled || prestigeEnabled || masteryTextEnabled)
                summonersQuery = summonersQuery.Include(s => s.ChampionMasteries);
            var summoners = summonersQuery.ToList();

            var classes = new List<string>();
            // re-add the old css classes, ignoring ones generated by the bot
            if (oldCss != null)
                classes.AddRange(oldCss.Split().Where(c => !string.IsNullOrWhiteSpace(c) && !c.StartsWith(rankPrefix)
                && !c.StartsWith(masteryPrefix) && !c.StartsWith(prestigePrefix) && !masteryTextClass.Equals(c)));

            if (rankEnabled)
            {
                var tier = (Tier) summoners.Select(s => s.Rank.Tier).DefaultIfEmpty().Max();
                classes.Add((rankPrefix + tier).ToLower());
            }

            if (masteryEnabled)
            {
                var masteryLevel = summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Level ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a > b ? a : b);

                if (masteryLevel > subreddit.MinimumChampionMasteryLevel)
                    classes.Add(masteryPrefix + masteryLevel);
            }

            // if subreddit has mastery text enabled, sanitize it
            if (subreddit.ChampionMasteryTextEnabled)
            {
                text = FlairUtil.SanitizeFlairTextLeadingMastery(text);
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
                        classes.Add(prestigePrefix + prestige);
                }

                if (masteryTextEnabled)
                {
                    text = FlairUtil.PrependFlairTextLeadingMastery(text, masteryPoints);
                    classes.Add(masteryTextClass);
                }
            }

            return new UserFlairParameter
            {
                Name = user.Name,
                CssClass = string.Join(" ", classes),
                Text = text
            };
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}