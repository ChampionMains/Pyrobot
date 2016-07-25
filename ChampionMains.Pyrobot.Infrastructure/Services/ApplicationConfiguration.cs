using System;

namespace ChampionMains.Pyrobot.Services
{
    public class ApplicationConfiguration
    {
        public TimeSpan LeagueDataStaleTime { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan FlairStaleTime { get; set; } = TimeSpan.FromHours(6);
        public string FlairBotVersion { get; set; } = "";
    }
}
