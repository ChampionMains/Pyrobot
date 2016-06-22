using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public class LeagueUtil
    {
        private static readonly string[] DivisionNames = new[]
        {
            "I", "II", "III", "IV", "V"
        };

        public static string Stringify(SummonerInfo league)
        {
            if (league == null) return "";
            if (league.UpdatedTime.HasValue == false) return "";
            if (league.Division == 0 || league.Tier == 0) return "Unranked";
            return league.Tier + " " + DivisionNames[league.Division - 1];
        }
    }
}