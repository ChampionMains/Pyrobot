using System.Text.RegularExpressions;

namespace ChampionMains.Pyrobot.Util
{
    public static class FlairUtil
    {
        // regex to remove things that are (or look like) numbers
        private const string FlairTextLeadingMasteryRegex = @"^\s*[\d,Ol]+\s*\b";

        public static bool CheckFlairTextLeadingMastery(string text)
        {
            return Regex.IsMatch(text, FlairTextLeadingMasteryRegex);
        }

        public static string SanitizeFlairTextLeadingMastery(string text)
        {
            return Regex.Replace(text, FlairTextLeadingMasteryRegex, "");
        }

        public static string PrependFlairTextLeadingMastery(string text, int mastery)
        {
            var masteryString = mastery.ToString("N0");
            return string.IsNullOrWhiteSpace(text) ? masteryString : $"{masteryString} {text}";
        }
    }
}
