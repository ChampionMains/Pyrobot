using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Reg = MingweiSamuel.Camille.Enums.Region;

namespace ChampionMains.Pyrobot.Models
{
    public class SummonerModel
    {
        public static readonly List<string> AllRegions = new List<string>
        {
            Reg.BR.Key,
            Reg.EUNE.Key,
            Reg.EUW.Key,
            Reg.NA.Key,
            // https://discussion.developer.riotgames.com/articles/2435/impending-removal-of-rune-page-names-from-kr-summo.html
            // Reg.KR.Key,
            Reg.LAN.Key,
            Reg.LAS.Key,
            Reg.OCE.Key,
            Reg.RU.Key,
            Reg.TR.Key,
            Reg.JP.Key
        };

//        static SummonerModel()
//        {
//            AllRegions.Remove("KR");
//        }

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

        /// <summary>
        /// Token from ValidationService.
        /// </summary>
        public string Token { get; set; }
    }
}