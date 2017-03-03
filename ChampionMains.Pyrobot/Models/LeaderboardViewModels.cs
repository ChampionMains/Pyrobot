using System.Collections.Generic;

namespace ChampionMains.Pyrobot.Models
{
    public class LeaderboardViewModel
    {
        public string SortedBy => "mastery";
        public int ChampionId { get; set; }
        public IList<LeaderboardEntryViewModel> Entries { get; set; }
    }

    public class LeaderboardEntryViewModel
    {
        public string Name { get; set; }
        public int TotalPoints { get; set; }
    }
}