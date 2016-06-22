using System.Collections.Generic;

namespace ChampionMains.Pyrobot.Riot
{
    public class League
    {
        public ICollection<LeagueEntry> Entries { get; set; }
        public string Name { get; set; }
        public string ParticipantId { get; set; }
        public QueueType Queue { get; set; }
        public TierType Tier { get; set; }
    }
}