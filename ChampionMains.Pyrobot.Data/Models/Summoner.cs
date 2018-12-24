using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("Summoner")]
    public class Summoner : CreatedModelBase
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        [StringLength(21)]
        [Required]
        public string Name { get; set; }
 
        [StringLength(5)]
        [Required]
        public string Region { get; set; }

        [Required]
        public int ProfileIconId { get; set; }
 
        /// <summary>
        /// Old V3 summoner id (obsolete).
        /// </summary>
        public long? SummonerId { get; set; }

        /// <summary>
        /// Encrypted summoner ID for V4.
        /// </summary>
        [StringLength(80)]
        public string SummonerIdEnc { get; set; }

        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual SummonerRank Rank { get; set; }

        public virtual ICollection<SummonerChampionMastery> ChampionMasteries { get; set; }

        public override string ToString()
        {
            return $"{{Summoner \"{Name}\" {Id}: {Region}/{SummonerId}/{SummonerIdEnc}}}";
        }
    }
}