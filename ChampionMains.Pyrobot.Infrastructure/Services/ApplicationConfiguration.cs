using System;

namespace ChampionMains.Pyrobot.Services
{
    public class ApplicationConfiguration
    {
        public TimeSpan RiotUpdateMin { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan RiotUpdateMax { get; set; } = TimeSpan.FromHours(6);
        public TimeSpan FlairUpdateMin { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan FlairUpdateMax { get; set; } = TimeSpan.FromHours(6);
        public string FlairBotVersion { get; set; } = "unspecified version";
    }
}
