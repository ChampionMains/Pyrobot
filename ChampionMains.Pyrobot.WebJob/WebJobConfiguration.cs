using System;

namespace ChampionMains.Pyrobot.WebJob
{
    public class WebJobConfiguration
    {
        public TimeSpan TimeoutBulkUpdate { get; set; } = TimeSpan.FromMinutes(5);
    }
}
