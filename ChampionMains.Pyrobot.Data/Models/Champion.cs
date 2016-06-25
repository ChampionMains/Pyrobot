using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    [Table("Champion")]
    public class Champion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public short Id { get; set; }

        [Index(IsUnique = true)]
        [StringLength(21)]
        [Required]
        public string Name { get; set; }
    }
}
