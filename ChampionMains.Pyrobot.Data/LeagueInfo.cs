using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hallam.RedditRankedFlairs.Data
{
    [Table("LeagueInfo")]
    public class LeagueInfo
    {
        public int Division { get; set; }

        [ForeignKey("Summoner")]
        public int Id { get; set; }

        public virtual Summoner Summoner { get; set; }

        public TierName Tier { get; set; }

        public DateTimeOffset? UpdatedTime { get; set; }
    }
}