using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hallam.RedditRankedFlairs.Data
{
    /// <summary>
    ///     Represents a Reddit sub which is registered.
    /// </summary>
    public class SubReddit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique = true), Required, StringLength(21)]
        public string Name { get; set; }
    }
}