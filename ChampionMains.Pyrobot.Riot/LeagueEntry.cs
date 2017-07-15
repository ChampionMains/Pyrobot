using Newtonsoft.Json;

namespace ChampionMains.Pyrobot.Riot
{
    public class LeagueEntry
    {
        public QueueType QueueType { get; set; }
        public TierType Tier { get; set; }
        [JsonProperty("rank")]
        public DivisionType Division { get; set; }
        [JsonProperty("freshBlood")]
        public bool IsFreshBlood { get; set; }
        [JsonProperty("hotStreak")]
        public bool IsHotStreak { get; set; }
        [JsonProperty("inactive")]
        public bool IsInactive { get; set; }
        [JsonProperty("veteran")]
        public bool IsVeteran { get; set; }
        public int LeaguePoints { get; set; }
        public int Losses { get; set; }
        public MiniSeries MiniSeries { get; set; }
        public string PlayerOrTeamId { get; set; }
        public string PlayerOrTeamName { get; set; }
        public int Wins { get; set; }
    }
}