using System;
using System.Data;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public static class RankUtil
    {
        private static readonly string[] DivisionNames = new[]
        {
            "I", "II", "III", "IV", "V"
        };

        public static string Stringify(SummonerRank rank)
        {
            if (rank == null) return "";
            if (rank.UpdatedTime.HasValue == false) return "";
            if (!Enum.IsDefined(typeof(Tier), rank.Tier)) return "";
            var tier = (Tier) rank.Tier;
            if (tier == Tier.Unranked) return tier.ToString();
            var division = DivisionNames.ElementAtOrDefault(rank.Division - 1);
            if (division == null) return "";
            return tier + " " + division;
        }

        public static readonly int[] PrestigeLevels = new[]
        {
            21600,
            50000,
            80000,
            125000,
            250000,
            500000,
            1000000
        };

        public static byte GetprestigeLevel(int points)
        {
            return (byte) PrestigeLevels.Where(i => i <= points).Count();
        }

        private const string RankPrefix = "rank-";
        private const string MasteryPrefix = "mastery-";

        public static string GenerateFlairCss(User user, int championId, bool rankEnabled, bool masteryEnabled, string oldFlair = "")
        {
            var classes = oldFlair.Split()
                .Where(c => !string.IsNullOrWhiteSpace(c) && !c.StartsWith(RankPrefix) && !c.StartsWith(MasteryPrefix)).ToList();

            if (rankEnabled)
            {
                var tier = (Tier) user.Summoners.Select(s => s.Rank.Tier).DefaultIfEmpty().Max();
                classes.Add((RankPrefix + tier).ToLower());
            }
            if (masteryEnabled)
            {
                var points = user.Summoners
                    .Select(s => s.ChampionMasteries.FirstOrDefault(m => m.ChampionId == championId)?.Points ?? 0).DefaultIfEmpty().Sum();
                classes.Add(MasteryPrefix + GetprestigeLevel(points));
            }

            return string.Join(" ", classes);
        }
    }
}