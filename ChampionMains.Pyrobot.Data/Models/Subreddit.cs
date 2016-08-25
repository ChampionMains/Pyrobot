using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("Subreddit")]
    public class Subreddit
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(21)]
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

        /// <summary>
        /// If true, users can only enable both or neither (rank, championmastery)
        /// 
        /// Note: not actually uesd
        /// </summary>
        [Required]
        public bool BindEnabled { get; set; }
        
        [Required]
        [ForeignKey("Champion")]
        public short ChampionId { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual ICollection<SubredditUserFlair> SubredditUserFlairs { get; set; }
    }
}