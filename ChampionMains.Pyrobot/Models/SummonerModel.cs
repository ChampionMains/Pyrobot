using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ChampionMains.Pyrobot.Data.Enums;

namespace ChampionMains.Pyrobot.Models
{
    public class SummonerModel
    {
        public static readonly List<string> AllRegions = RegionUtils.GetRegionStrings();
        static SummonerModel()
        {
            // https://discussion.developer.riotgames.com/articles/2435/impending-removal-of-rune-page-names-from-kr-summo.html
            AllRegions.Remove("KR");
        }

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