using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChampionMains.Pyrobot.Models
{
    public class SummonerModel
    {
        public static readonly string[] AllRegions =
        {
            "BR", "EUNE", "EUW", "KR", "LAN", "LAS", "NA", "OCE",
            "TR", "RU"
        };

        public class RegionValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var s = value as string;
                if (string.IsNullOrEmpty(s))
                {
                    return new ValidationResult("Region is required");
                }
                return AllRegions.Contains(s, StringComparer.OrdinalIgnoreCase)
                    ? ValidationResult.Success
                    : new ValidationResult("Invalid Region");
            }
        }

        [RegionValidation]
        public string Region { get; set; } 

        [Required(ErrorMessage = "Summoner name is required")]
        public string SummonerName { get; set; }
    }
}