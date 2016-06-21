using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Riot;
using Hallam.RedditRankedFlairs.Services.Riot;
using Newtonsoft.Json.Linq;

namespace Hallam.RedditRankedFlairs.Services
{
    public class RiotService : IRiotService
    {
        private const string LeagueBaseUri = "v2.5/league/";
        private const string SummonerBaseUri = "v1.4/summoner/";

        public RiotWebRequester WebRequester { get; set; }

        public async Task<Summoner> FindSummonerAsync(string region, string summonerName)
        {
            var uri = SummonerBaseUri + "by-name/" + Uri.EscapeDataString(summonerName);
            var json = await WebRequester.SendRequestAsync(region, uri);
            var results = json?.ToObject<IDictionary<string, Summoner>>();
            return results?.Count > 0 ? results.First().Value : null;
        }

        public async Task<ICollection<League>> GetLeaguesAsync(string region, int summonerId)
        {
            var uri = LeagueBaseUri + "by-summoner/" + summonerId + "/entry";
            var json = await WebRequester.SendRequestAsync(region, uri);
            if (json == null) return null;
            var results = json.ToObject<IDictionary<string, ICollection<League>>>();
            ICollection<League> result;
            return results.TryGetValue(summonerId.ToString(), out result) ? result : null;
        }

        public async Task<ICollection<RunePage>> GetRunePagesAsync(string region, int summonerId)
        {
            var uri = SummonerBaseUri + summonerId + "/runes";
            var json = await WebRequester.SendRequestAsync(region, uri);
            var result = json?[summonerId.ToString()];
            return result?["pages"].ToObject<ICollection<RunePage>>();
        }
    }
}