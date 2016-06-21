using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hallam.RedditRankedFlairs.Data
{
    /// <summary>
    ///     Represents a user of the product.
    /// </summary>
    public class User
    {
        public DateTimeOffset? FlairUpdateRequiredTime { get; set; }
        public DateTimeOffset? FlairUpdatedTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsBanned { get; set; }

        public bool IsAdmin { get; set; }

        [Index(IsUnique = true), Required, StringLength(21)]
        public string Name { get; set; }
        
        public virtual ICollection<Summoner> Summoners { get; set; } 
    }
}