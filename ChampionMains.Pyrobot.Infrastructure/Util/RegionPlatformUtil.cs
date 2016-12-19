using System;
using RiotSharp;

namespace ChampionMains.Pyrobot.Startup
{
    public static class RegionPlatformUtil
    {
        public static Platform ConvertRegionToPlatform(Region region)
        {
            switch (region)
            {
                case Region.br:
                    return Platform.BR1;
                case Region.eune:
                    return Platform.EUN1;
                case Region.euw:
                    return Platform.EUW1;
                case Region.na:
                    return Platform.NA1;
                case Region.kr:
                    return Platform.KR;
                case Region.lan:
                    return Platform.LA1;
                case Region.las:
                    return Platform.LA2;
                case Region.oce:
                    return Platform.OC1;
                case Region.ru:
                    return Platform.RU;
                case Region.tr:
                    return Platform.TR1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }
    }
}
