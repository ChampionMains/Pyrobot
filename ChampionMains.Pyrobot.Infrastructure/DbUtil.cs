using System;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs
{
    public class DbUtil
    {
        public static Func<Summoner, bool> CreateComparer(string region, string summonerName, bool ignoreCase = true)
        {
            var comparisonType = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            return
                summoner =>
                    summoner.Region.Equals(region, comparisonType) && summoner.Name.Equals(summonerName, comparisonType);
        }  
    }
}