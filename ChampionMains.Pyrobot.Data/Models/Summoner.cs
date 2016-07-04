using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("Summoner")]
    public class Summoner
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [StringLength(21)]
        [Required]
        public string Name { get; set; }

        [Index("IX_Region_SummonerId", 1, IsUnique = true)]
        [StringLength(5)]
        [Required]
        public string Region { get; set; }
        
        [Required]
        public int ProfileIconId { get; set; }

        [Index("IX_Region_SummonerId", 2, IsUnique = true)]
        [Required]
        public long SummonerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }

        public virtual SummonerRank Rank { get; set; }

        public virtual ICollection<SummonerChampionMastery> ChampionMasteries { get; set; }
    }
}