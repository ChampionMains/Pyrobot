using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    //[Table("dbo.SummonerChampionMastery")]
    public class SummonerChampionMastery
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int MasteryPoints { get; set; }

        [Required]
        public byte MasteryLevel { get; set; }

        [ForeignKey("Champion")]
        [Required]
        public short ChampionId { get; set; }


        [ForeignKey("SummonerInfo")]
        [Required]
        public int SummonerInfoId { get; set; }

        public virtual Champion Champion { get; set; }

        public virtual SummonerInfo SummonerInfo{ get; set; }
    }
}