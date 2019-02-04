using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("User")]
    public class User : CreatedModelBase
    {
        //[NotMapped]
        //public DateTimeOffset? FlairUpdateRequiredTime { get; set; }
        //[NotMapped]
        //public DateTimeOffset? FlairUpdatedTime { get; set; }

        [Required]
        [Key]
        public int Id { get; set; }
        
        [Index(IsUnique = true)]
        [Required]
        [StringLength(21)]
        public string Name { get; set; }

        [Required]
        public bool IsBanned { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        public int BackgroundSkinId { get; set; }

        public virtual ICollection<Summoner> Summoners { get; set; } = new HashSet<Summoner>();

        public virtual ICollection<SubredditUserFlair> SubredditUserFlairs { get; set; } = new HashSet<SubredditUserFlair>();
    }
}