using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SubredditUserFlair")]
    public class SubredditUserFlair
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        [Required]
        public bool RankEnabled { get; set; }

        [Required]
        public bool ChampionMasteryEnabled { get; set; }

        [Required]
        public bool PrestigeEnabled { get; set; }

        [Required]
        public bool ChampionMasteryTextEnabled { get; set; }

        [MaxLength(64)]
        public string FlairText { get; set; }

        [Index("IX_Subreddit_User", 1, IsUnique = true)]
        [ForeignKey("Subreddit")]
        [Required]
        public int SubredditId { get; set; }
        
        [Index("IX_Subreddit_User", 2, IsUnique = true)]
        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        public virtual Subreddit Subreddit{ get; set; }

        public virtual User User { get; set; }
    }
}