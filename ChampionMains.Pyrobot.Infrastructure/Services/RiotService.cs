using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services.Riot;
using Newtonsoft.Json;
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
            var json = await WebRequester.SendRequestAsync(region, uri);
            var results = JsonConvert.DeserializeObject<IDictionary<string, Summoner>>(json);
            return results?.Count > 0 ? results.First().Value : null;
        }

        public async Task<Tuple<Tiers, byte>> GetLeaguesAsync(string region, long summonerId)
        {
            var uri = LeagueBaseUri + "by-summoner/" + summonerId + "/entry";
            var json = await WebRequester.SendRequestAsync(region, uri);
            if (json == null) return Tuple.Create(Tiers.Unranked, (byte)0);

            var results = JsonConvert.DeserializeObject<IDictionary<string, ICollection<League>>>(json);
            ICollection<League> result;
            var leagues = results.TryGetValue(summonerId.ToString(), out result) ? result : null;

            var solo = leagues?.FirstOrDefault(league => league.Queue == QueueType.RANKED_SOLO_5x5);

            if (solo == null) return Tuple.Create(Tiers.Unranked, (byte) 0);

            var entry = solo.Entries.First(e => e.PlayerOrTeamId == summonerId.ToString());
            var division = (byte) entry.Division;
            var tier = (Tiers) Enum.Parse(typeof(Tiers), solo.Tier.ToString(), true);
            return Tuple.Create(tier, division);
        }

        public async Task<ICollection<ChampionMastery>> GetChampionMasteriesAsync(string region, long summonerId)
        {
            var uri = "player/" + summonerId + "/champions";
            var json = await WebRequester.SendRequestAsync(region, uri, innerUri: "championmastery/location", usePlatform: true);

            return JsonConvert.DeserializeObject<ICollection<ChampionMastery>>(json);
        }

        public async Task<ICollection<RunePage>> GetRunePagesAsync(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId + "/runes";
            var json = await WebRequester.SendRequestAsync(region, uri);

            var result = JsonConvert.DeserializeObject<IDictionary<string, RunePageResponse>>(json);
            return result[summonerId.ToString()].Pages;
        }
    }
}