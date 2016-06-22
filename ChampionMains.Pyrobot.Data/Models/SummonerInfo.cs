using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("dbo.SummonerInfo")]
    public class SummonerInfo
    {
        [Key]
        [ForeignKey("dbo.Summoner")]
        [Required]
        public int Id { get; set; }

        [Required]
        public byte Division { get; set; }

        [Required]
        public byte Tier { get; set; }

        [Required]
        public DateTimeOffset? UpdatedTime { get; set; }

        public virtual Summoner Summoner { get; set; }

        public virtual ICollection<SummonerChampionMastery> SummonerChampionMasteries { get; set; }
    }
}