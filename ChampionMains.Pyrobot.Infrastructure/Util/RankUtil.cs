using System;
using System.Collections.Generic;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Data.Models;
using RiotSharp;

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

        public static Tuple<byte, byte> GetHighestLeague(List<RiotSharp.LeagueEndpoint.League> leagues, long summonerId)
        {
            var league = leagues?.Where(l => l.Queue == Queue.RankedSolo5x5 || l.Queue == Queue.RankedFlexSR)
                                    .OrderBy(l => l.Tier, RankUtil.TierComparer)
                                    .FirstOrDefault();
            byte? division = null;
            if (league != null)
            {
                division = RankUtil.DivisionNumeralToByte(
                    league.Entries.First(e => e.PlayerOrTeamId.Equals(summonerId.ToString())).Division);
            }
            return Tuple.Create(RankUtil.TierToByte(league?.Tier), division ?? 0);
        }

        public static byte DivisionNumeralToByte(string numeral)
        {
            numeral = numeral.ToUpperInvariant();
            if (numeral[0] == 'I')
            {
                if (numeral.Length != 2) // I, III
                    return (byte) numeral.Length;
                if (numeral[1] == 'I') // II
                    return 2;
                return 4; // IV
            }
            return 5;
        }

        public static byte TierToByte(RiotSharp.LeagueEndpoint.Enums.Tier? tier)
        {
            switch (tier)
            {
                case RiotSharp.LeagueEndpoint.Enums.Tier.Challenger:
                    return 7;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Master:
                    return 6;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Diamond:
                    return 5;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Platinum:
                    return 4;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Gold:
                    return 3;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Silver:
                    return 2;
                case RiotSharp.LeagueEndpoint.Enums.Tier.Bronze:
                    return 1;
                default: // unranked, null
                    return 0;
            }
        }

        public static readonly IComparer<RiotSharp.LeagueEndpoint.Enums.Tier> TierComparer = Comparer<RiotSharp.LeagueEndpoint.Enums.Tier>.Create(
            (a, b) => TierToByte(b) - TierToByte(a)); // Reversed to be in correct order.
    }
}