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

        [StringLength(5)]
        [Required]
        public string Region { get; set; }
        
        [Required]
        public long SummonerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }

        public virtual SummonerInfo SummonerInfo { get; set; }
    }
}