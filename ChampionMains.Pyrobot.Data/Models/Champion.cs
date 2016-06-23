using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChampionMains.Pyrobot.Data.Models
{
    //[Table("dbo.Champion")]
    public class Champion
    {
        [Key]
        [Required]
        public short Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
