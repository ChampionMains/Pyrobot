using System;
using System.Collections.Generic;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public static class RankUtil
    {
        private static readonly string[] DivisionNames = {
            "I", "II", "III", "IV", "V"
        };

        public static string Stringify(SummonerRank rank)
        {
            if (rank == null) return "";
            //if (rank.UpdatedTime.HasValue == false) return "";
            if (!Enum.IsDefined(typeof(Tier), rank.Tier)) return "";
            var tier = (Tier) rank.Tier;
            if (tier == Tier.Unranked) return tier.ToString();
            var division = DivisionNames.ElementAtOrDefault(rank.Division - 1);
            if (division == null) return "";
            return tier + " " + division;
        }

        // must be ascending order
        public static readonly int[] PrestigeLevels = new[]
        {
            250000, 500000, 750000, 1000000, 2000000
        };

        public static int GetPrestigeLevel(int points)
        {
            return PrestigeLevels.Reverse().FirstOrDefault(p => p <= points);
        }

        public static int GetNextPrestigeLevel(int points)
        {
            return PrestigeLevels.FirstOrDefault(p => p > points);
        }

        private const string RankPrefix = "rank-";
        private const string MasteryPrefix = "mastery-";
        private const string PrestigePrefix = "prestige-";

        public static string GenerateFlairCss(User user, int championId, bool rankEnabled,
            bool masteryEnabled, bool prestigeEnabled, string oldCss)
        {
            var classes = new List<string>();
            if (oldCss != null)
                classes.AddRange(oldCss.Split().Where(c => !string.IsNullOrWhiteSpace(c)
                    && !c.StartsWith(RankPrefix) && !c.StartsWith(MasteryPrefix) && !c.StartsWith(PrestigePrefix)));

            if (rankEnabled)
            {
                var tier = (Tier) user.Summoners.Select(s => s.Rank.Tier).DefaultIfEmpty().Max();
                classes.Add((RankPrefix + tier).ToLower());
            }

            if (masteryEnabled || prestigeEnabled)
            {
                var masteryLevel = user.Summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Level ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a > b ? a : b);

                if (masteryEnabled && masteryLevel > 0)
                    classes.Add(MasteryPrefix + masteryLevel);

                var masteryPoints = user.Summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Points ?? 0)
                    .DefaultIfEmpty().Aggregate((a, b) => a + b);
                var prestige = GetPrestigeLevel(masteryPoints) / 1000;

                if (prestige > 0)
                    classes.Add(PrestigePrefix + prestige);
            }

            return string.Join(" ", classes);
        }
    }
}