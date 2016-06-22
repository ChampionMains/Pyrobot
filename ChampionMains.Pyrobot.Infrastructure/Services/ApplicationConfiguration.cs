using System;

namespace ChampionMains.Pyrobot.Services
{
    public class ApplicationConfiguration
    {
        public TimeSpan LeagueDataStaleTime { get; set; } = TimeSpan.FromHours(4);
        public string FlairBotVersion { get; set; } = "";
    }
}
