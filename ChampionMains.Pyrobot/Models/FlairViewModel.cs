using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ChampionMains.Pyrobot.Models
{
    public class FlairViewModel
    {
        [Required]
        [JsonProperty(PropertyName = "subreddit")]
        public string Subreddit { get; set; }

        [Required]
        [JsonProperty(PropertyName = "rankEnabled")]
        public bool RankEnabled { get; set; }

        [Required]
        [JsonProperty(PropertyName = "championMasteryEnabled")]
        public bool ChampionMasteryEnabled { get; set; }

        [JsonProperty(PropertyName = "flairText")]
        public string FlairText { get; set; }
    }
}