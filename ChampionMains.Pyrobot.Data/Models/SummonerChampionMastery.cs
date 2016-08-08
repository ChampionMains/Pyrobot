using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SummonerChampionMastery")]
    public class SummonerChampionMastery
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public byte Level { get; set; }

        [Index("IX_Champion_Summoner", 1, IsUnique = true)]
        [ForeignKey("Champion")]
        [Required]
        public short ChampionId { get; set; }

        [Index("IX_Champion_Summoner", 2, IsUnique = true)]
        [Index("IX_Summoner")]
        [ForeignKey("Summoner")]
        [Required]
        public int SummonerId { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual Summoner Summoner { get; set; }
    }
}