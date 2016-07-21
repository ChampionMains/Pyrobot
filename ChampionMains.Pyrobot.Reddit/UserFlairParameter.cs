using Newtonsoft.Json;

namespace ChampionMains.Pyrobot.Reddit
{
    public class UserFlairParameter
    {
        [JsonProperty("flair_css_class")]
        public string CssClass { get; set; }

        public string Name { get; set; }

        [JsonProperty("flair_text")]
        public string Text { get; set; }
    }
}