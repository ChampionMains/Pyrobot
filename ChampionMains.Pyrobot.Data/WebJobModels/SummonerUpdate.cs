using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Data.WebJobModels
{
    public class SummonerUpdate
    {
        public string Region { get; set; }
        public long SummonerId { get; set; }
    }
}
