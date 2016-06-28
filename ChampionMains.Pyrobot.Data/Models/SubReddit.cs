using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SubReddit")]
    public class SubReddit
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

        /// <summary>
        /// If true, users can only enable both or neither (rank, championmastery)
        /// </summary>
        [Required]
        public bool BindEnabled { get; set; }
        
        [Required]
        [ForeignKey("Champion")]
        public short ChampionId { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual ICollection<SubRedditUser> SubRedditUsers { get; set; }
    }
}