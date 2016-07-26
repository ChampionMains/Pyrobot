using System;
using ChampionMains.Pyrobot.Data.Models;

namespace ChampionMains.Pyrobot
{
    public class DbUtil
    {
        public static Func<Summoner, bool> CreateComparer(string region, string summonerName, bool ignoreCase = true)
        {
            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return
                summoner =>
                    summoner.Region.Equals(region, comparisonType) && summoner.Name.Equals(summonerName, comparisonType);
        }  
    }
}