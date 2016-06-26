using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SubRedditUser")]
    public class SubRedditUser
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public bool RankEnabled { get; set; }

        [Required]
        public bool ChampionMasteryEnabled { get; set; }

        [Index("IX_SubReddit_User", 1, IsUnique = true)]
        [ForeignKey("SubReddit")]
        [Required]
        public int SubRedditId { get; set; }
        
        [Index("IX_SubReddit_User", 2, IsUnique = true)]
        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        public virtual SubReddit SubReddit { get; set; }

        public virtual User User { get; set; }
    }
}