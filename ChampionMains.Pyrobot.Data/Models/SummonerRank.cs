using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SummonerRank")]
    public class SummonerRank
    {
        [ForeignKey("Summoner")]
        [Required]
        public int Id { get; set; }

        [Required]
        public byte Division { get; set; }

        [Required]
        public byte Tier { get; set; }
        
        public virtual Summoner Summoner { get; set; }
    }
}