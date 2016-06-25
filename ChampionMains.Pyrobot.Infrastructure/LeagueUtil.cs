using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public class LeagueUtil
    {
        private static readonly string[] TierNames = new[]
        {
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Challenger"
        };
        private static readonly string[] DivisionNames = new[]
        {
            "I", "II", "III", "IV", "V"
        };

        public static string Stringify(SummonerInfo league)
        {
            if (league == null) return "";
            if (league.UpdatedTime.HasValue == false) return "";
            if (league.Division == 0 || league.Tier == 0) return "Unranked";
            return TierNames[league.Tier - 1] + " " + DivisionNames[league.Division - 1];
        }
    }
}