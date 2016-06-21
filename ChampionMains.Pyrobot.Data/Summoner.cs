using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hallam.RedditRankedFlairs.Data
{
    /// <summary>
    ///     Represents a summoner registration. The summoner is owned by a user.
    /// </summary>
    public class Summoner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public virtual LeagueInfo LeagueInfo { get; set; }

        [Index("IX_RegionSummonerName", 1, IsUnique = true), Required, StringLength(21)]
        public string Name { get; set; }

        [Index("IX_RegionSummonerName", 0, IsUnique = true), Required, StringLength(5)]
        public string Region { get; set; }
        
        public int SummonerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }
    }
}