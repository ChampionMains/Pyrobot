using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services.Riot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Summoner = ChampionMains.Pyrobot.Data.Models.Summoner;

namespace ChampionMains.Pyrobot.Services
{
    public class RiotService
    {
        private const string LeagueBaseUri = "v2.5/league/";
        private const string SummonerBaseUri = "v1.4/summoner/";

        public RiotWebRequester WebRequester { get; set; }

        public async Task<Summoner> FindSummonerAsync(string region, string summonerName)
        {
            var uri = SummonerBaseUri + "by-name/" + Uri.EscapeDataString(summonerName);
            return GetSummonerFromResponse(await WebRequester.SendRequestAsync(region, uri));
        }

        public async Task<Summoner> FindSummonerAsync(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId;
            return GetSummonerFromResponse(await WebRequester.SendRequestAsync(region, uri));
        }

        private Summoner GetSummonerFromResponse(JToken json)
        {
            var results = json?.ToObject<IDictionary<string, Summoner>>();
            return results?.Count > 0 ? results.First().Value : null;
        }


        public async Task<Tuple<Tier, byte>> GetLeaguesAsync(string region, long summonerId)
        {
            var uri = LeagueBaseUri + "by-summoner/" + summonerId + "/entry";
            var json = await WebRequester.SendRequestAsync(region, uri);
            if (json == null) return Tuple.Create(Tier.Unranked, (byte)0);
            
            var leagues = json[summonerId.ToString()]?.ToObject<ICollection<League>>();

            var solo = leagues?.FirstOrDefault(league => league.Queue == QueueType.RANKED_SOLO_5x5);

            if (solo == null) return Tuple.Create(Tier.Unranked, (byte) 0);

            var entry = solo.Entries.First(e => e.PlayerOrTeamId == summonerId.ToString());
            var division = (byte) entry.Division;
            var tier = (Tier) Enum.Parse(typeof(Tier), solo.Tier.ToString(), true);
            return Tuple.Create(tier, division);
        }

        public async Task<ICollection<ChampionMastery>> GetChampionMasteriesAsync(string region, long summonerId)
        {
            var uri = "player/" + summonerId + "/champions";
            var json = await WebRequester.SendRequestAsync(region, uri, innerUri: "championmastery/location", usePlatform: true);

            return json.ToObject<ICollection<ChampionMastery>>();
        }

        public async Task<ICollection<RunePage>> GetRunePagesAsync(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId + "/runes";
            var json = await WebRequester.SendRequestAsync(region, uri);

            return json[summonerId.ToString()]?["pages"]?.ToObject<ICollection<RunePage>>();
        }
    }
}