﻿using System;

namespace Hallam.RedditRankedFlairs.Services
{
    public class ApplicationConfiguration
    {
        public TimeSpan LeagueDataStaleTime { get; set; } = TimeSpan.FromHours(4);
    }
}
