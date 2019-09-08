using System;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot.Util
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
        public static readonly int[] PrestigeLevels = {
            250000, 500000, 750000, 1000000, 2000000, 3000000, 4000000, 5000000
        };

        public static int GetPrestigeLevel(int points)
        {
            return PrestigeLevels.Reverse().FirstOrDefault(p => p <= points);
        }

        public static int GetNextPrestigeLevel(int points)
        {
            return PrestigeLevels.FirstOrDefault(p => p > points);
        }
    }
}
