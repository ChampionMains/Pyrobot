using System;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public class RankUtil
    {
        private static readonly string[] DivisionNames = new[]
        {
            "I", "II", "III", "IV", "V"
        };

        public static string Stringify(SummonerRank rank)
        {
            if (rank == null) return "";
            if (rank.UpdatedTime.HasValue == false) return "";
            if (!Enum.IsDefined(typeof(Tiers), rank.Tier)) return "";
            var tier = (Tiers) rank.Tier;
            if (tier == Tiers.Unranked) return tier.ToString();
            var division = DivisionNames.ElementAtOrDefault(rank.Division - 1);
            if (division == null) return "";
            return tier + " " + division;
        }
    }
}