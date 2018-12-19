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

        /// <summary>
        /// <see cref="Enums.Division"/>
        /// </summary>
        [Required]
        public byte Division { get; set; }

        /// <summary>
        /// <see cref="Enums.Tier"/>
        /// </summary>
        [Required]
        public byte Tier { get; set; }
        
        public virtual Summoner Summoner { get; set; }
    }
}