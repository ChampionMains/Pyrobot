using System;
using System.Collections.Generic;
using System.Linq;

namespace ChampionMains.Pyrobot.Data.Enums
{
    public enum Region
    {
        BR, EUNE, EUW, JP, KR, LAN, LAS, NA, OCE, RU, TR
    }

    public static class RegionUtils
    {
        public static IEnumerable<Region> GetRegions()
        {
            return Enum.GetValues(typeof(Region)).Cast<Region>();
        }

        public static IEnumerable<string> GetRegionStrings()
        {
            return GetRegions().Select(x => x.ToString());
        }
    }
}
