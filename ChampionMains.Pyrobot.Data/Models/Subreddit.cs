using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("Subreddit")]
    public class Subreddit : CreatedModelBase
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(21)]
        public string Name { get; set; }

        [Required]
        public bool AdminOnly { get; set; }

        [Required]
        public bool RankEnabled { get; set; }

        [Required]
        public bool ChampionMasteryEnabled { get; set; }

        [Required]
        public bool PrestigeEnabled { get; set; }

        [Required]
        public bool ChampionMasteryTextEnabled { get; set; }

        [Required]
        // TODO: not implemented
        public bool TextDisabled { get; set; }

        [Required]
        public byte MinimumChampionMasteryLevel { get; set; }

        [MaxLength]
        public string FlairCss { get; set; }

        /// <summary>
        /// If true, users can only enable both or neither (rank, championmastery).
        /// 
        /// Note: not actually used.
        /// </summary>
        [Required]
        public bool BindEnabled { get; set; }

        /// <summary>
        /// True if the bot doesn't have mod access to this subreddit.
        /// </summary>
        [Required]
        public bool MissingMod { get; set; }

        /// <summary>
        /// Last time flair bulk update triggered on this subreddit.
        /// </summary>
        public DateTimeOffset? LastBulkUpdate { get; set; }

        [Required]
        [ForeignKey("Champion")]
        public short ChampionId { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual ICollection<SubredditUserFlair> SubredditUserFlairs { get; set; }
    }
}
