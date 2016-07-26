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

        [ForeignKey("Champion")]
        [Required]
        public short ChampionId { get; set; }

        [ForeignKey("Summoner")]
        [Required]
        public int SummonerId { get; set; }

        public DateTimeOffset? UpdatedTime { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual Summoner Summoner { get; set; }
    }
}