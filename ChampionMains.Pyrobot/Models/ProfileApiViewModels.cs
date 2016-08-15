using System;
using System.Collections.Generic;

namespace ChampionMains.Pyrobot.Models
{
    public class ApiDataViewModel
    {
        public IDictionary<int, SummonerDataViewModel> Summoners { get; set; }
        public IDictionary<short, ChampionMasteryDataViewModel> Champions { get; set; }
        public IDictionary<int, SubredditDataViewModel> Subreddits { get; set; }
    }

    public class SummonerDataViewModel
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public int ProfileIcon { get; set; }
        public string Rank { get; set; }
        public byte Tier { get; set; }
        public string TierString { get; set; }
        public byte Division { get; set; }
        public int TotalPoints { get; set; }
        public DateTimeOffset? LastUpdate { get; set; }
    }

    public class ChampionMasteryDataViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public int Points { get; set; }
        public byte Level { get; set; }
        public int? Prestige { get; set; }
        public int? NextPrestige { get; set; }
    }

    public class SubredditDataViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short ChampionId { get; set; }

        public bool AdminOnly { get; set; }
        public bool RankEnabled { get; set; }
        public bool ChampionMasteryEnabled { get; set; }
        public bool PrestigeEnabled { get; set; }
        public bool BindEnabled { get; set; }

        public SubredditUserDataViewModel Flair { get; set; }
    }

    public class SubredditUserDataViewModel
    {
        public int SubredditId { get; set; }

        public bool RankEnabled { get; set; }
        public bool ChampionMasteryEnabled { get; set; }
        public bool PrestigeEnabled { get; set; }
        public string FlairText { get; set; }
    }
}