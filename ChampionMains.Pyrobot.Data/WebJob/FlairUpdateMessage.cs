namespace ChampionMains.Pyrobot.Data.WebJob
{
    public class FlairUpdateMessage
    {
        public int UserId { get; set; }
        public int SubredditId { get; set; }

        public bool RankEnabled { get; set; }
        public bool ChampionMasteryEnabled { get; set; }
        public bool PrestigeEnabled { get; set; }
        public bool ChampionMasteryTextEnabled { get; set; }

        public string FlairText { get; set; }
    }
}
