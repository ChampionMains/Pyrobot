using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("SubReddit")]
    public class SubReddit
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(21)]
        public string Name { get; set; }
        
        [ForeignKey("Champion")]
        public short? ChampionId { get; set; }

        public virtual Champion Champion { get; set; }
    }
}