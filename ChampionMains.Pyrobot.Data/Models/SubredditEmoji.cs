using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SubredditEmoji")]
    public class SubredditEmoji
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Subreddit")]
        [Required]
        public int SubredditId { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        [MaxLength(24)] // Max emoji name length.
        public string Name { get; set; }

        /// <summary>
        /// <see cref="Enums.Tier"/>
        /// Has custom unique nullable index.
        /// </summary>
        public byte? Tier { get; set; }
        
        public virtual Subreddit Subreddit { get; set; }
    }
}
