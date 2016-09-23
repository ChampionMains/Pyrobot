using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using ChampionMains.Pyrobot.Riot;
using ChampionMains.Pyrobot.Services.Riot;
using Newtonsoft.Json.Linq;
using Summoner = ChampionMains.Pyrobot.Riot.Summoner;

namespace ChampionMains.Pyrobot.Services
{
    public class RiotService
    {
        private const int MaxSummonerRequestSize = 40;
        private const int MaxLeagueRequestSize = 10;

        private const string LeagueBaseUri = "v2.5/league/";
        private const string SummonerBaseUri = "v1.4/summoner/";

        public RiotWebRequester WebRequester { get; set; }

        public async Task<Summoner> GetSummoner(string region, string summonerName)
        {
            var uri = SummonerBaseUri + "by-name/" + Uri.EscapeDataString(summonerName);
            return GetSummonersFromResponse(await WebRequester.SendRequestAsync(region, uri)).First();
        }

        public async Task<Summoner> GetSummoner(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId;
            return GetSummonersFromResponse(await WebRequester.SendRequestAsync(region, uri)).First();
        }

        public async Task<IDictionary<long, Summoner>> GetSummoners(string region, ICollection<long> summonerIds)
        {
            var data = summonerIds.Select((id, i) => new {id, g = i/MaxSummonerRequestSize}).GroupBy(x => x.g)
                .Select(async group =>
                {
                    var uri = SummonerBaseUri + group.Select(x => x.id.ToString()).Aggregate((a, b) => a + ',' + b);
                    return GetSummonersFromResponse(await WebRequester.SendRequestAsync(region, uri));
                }).ToList();
            await Task.WhenAll(data);
            try
            {
                return data.SelectMany(d => d.Result).ToDictionary(s => s.Id, s => s);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Summoner Ids: " + summonerIds.ToString(), e);
            }
        }

        public async Task<Tuple<Tier, byte>> GetRank(string region, long summonerId)
        {
            var uri = LeagueBaseUri + "by-summoner/" + summonerId + "/entry";
            return GetRanksFromResponse(await WebRequester.SendRequestAsync(region, uri))?.First().Value;
        }

        public async Task<IDictionary<long, Tuple<Tier, byte>>> GetRanks(string region, ICollection<long> summonerIds)
        {
            var data = summonerIds.Select((id, i) => new {id, g = i/MaxLeagueRequestSize}).GroupBy(x => x.g)
                .Select(async group =>
                {
                    var uri = LeagueBaseUri + "by-summoner/" + group.Select(x => x.id.ToString()).Aggregate((a, b) => a + ',' + b) + "/entry";
                    return GetRanksFromResponse(await WebRequester.SendRequestAsync(region, uri)) ?? new Dictionary<long, Tuple<Tier, byte>>();
                }).ToList();
            await Task.WhenAll(data);
            return data.SelectMany(d => d.Result).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<ICollection<ChampionMastery>> GetChampionMastery(string region, long summonerId)
        {
            var uri = "player/" + summonerId + "/champions";
            var json = await WebRequester.SendRequestAsync(region, uri, innerUri: "championmastery/location", usePlatform: true);

            return json.ToObject<ICollection<ChampionMastery>>();
        }

        public async Task<IDictionary<long, ICollection<ChampionMastery>>> GetChampionMasteries(string region,
            ICollection<long> summonerIds)
        {
            var data = summonerIds.ToDictionary(id => id, async id => await GetChampionMastery(region, id));
            await Task.WhenAll(data.Values);
            return data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result);
        }

        public async Task<ICollection<RunePage>> GetRunePages(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId + "/runes";
            var json = await WebRequester.SendRequestAsync(region, uri);

            return json[summonerId.ToString()]?["pages"]?.ToObject<ICollection<RunePage>>();
        }

        private static IList<Summoner> GetSummonersFromResponse(JToken json)
        {
            return json.ToObject<IDictionary<string, Summoner>>().Values.ToList();
        }

        private static IDictionary<long, Tuple<Tier, byte>> GetRanksFromResponse(JToken json)
        {
            var data = json?.ToObject<IDictionary<long, ICollection<League>>>();

            return data?.ToDictionary(kvp => kvp.Key,
                kvp =>
                {
                    var solo = kvp.Value.FirstOrDefault(l => l.Queue == QueueType.RANKED_SOLO_5x5);

                    if (solo == null) return Tuple.Create(Tier.Unranked, (byte)0);

                    var entry = solo.Entries.First(e => e.PlayerOrTeamId == kvp.Key.ToString());
                    var division = (byte)entry.Division;
                    var tier = (Tier)Enum.Parse(typeof(Tier), solo.Tier.ToString(), true);
                    return Tuple.Create(tier, division);
                });
        }

    }
}