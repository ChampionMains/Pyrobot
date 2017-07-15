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
        private const string LeagueBaseUri = "league/v3/positions/by-summoner/";
        private const string SummonerBaseUri = "summoner/v3/summoners/";
        private const string ChampionMasteryBaseUri = "champion-mastery/v3/champion-masteries/by-summoner/";
        private const string RunesBaseUri = "platform/v3/runes/by-summoner/";

        public RiotWebRequester WebRequester { get; set; }

        public async Task<Summoner> GetSummoner(string region, string summonerName)
        {
            var uri = SummonerBaseUri + "by-name/" + Uri.EscapeDataString(summonerName);
            return (await WebRequester.SendRequestAsync(region, uri)).ToObject<Summoner>();
        }

        public async Task<Summoner> GetSummoner(string region, long summonerId)
        {
            var uri = SummonerBaseUri + summonerId;
            return (await WebRequester.SendRequestAsync(region, uri)).ToObject<Summoner>();
        }

        public async Task<IDictionary<long, Summoner>> GetSummoners(string region, ICollection<long> summonerIds)
        {
            var tasks = summonerIds.Select(async id => new KeyValuePair<long, Summoner>(id, await GetSummoner(region, id))).ToList();
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Tuple<Tier, byte>> GetRank(string region, long summonerId)
        {
            var uri = LeagueBaseUri + summonerId;
            return GetRankFromResponse(await WebRequester.SendRequestAsync(region, uri));
        }

        public async Task<IDictionary<long, Tuple<Tier, byte>>> GetRanks(string region, ICollection<long> summonerIds)
        {
            var tasks = summonerIds.Select(async id => new KeyValuePair<long, Tuple<Tier, byte>>(id, await GetRank(region, id)));
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<ICollection<ChampionMastery>> GetChampionMastery(string region, long summonerId)
        {
            var uri = ChampionMasteryBaseUri + summonerId;
            var json = await WebRequester.SendRequestAsync(region, uri);
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
            var uri = RunesBaseUri + summonerId;
            var json = await WebRequester.SendRequestAsync(region, uri);

            return json?["pages"].ToObject<ICollection<RunePage>>();
        }

        private static Tuple<Tier, byte> GetRankFromResponse(JToken json)
        {
            var entry = json?.ToObject<IList<LeagueEntry>>()
                .FirstOrDefault(e => QueueType.RANKED_SOLO_5x5 == e.QueueType);
            if (entry == null)
                return null;
            var division = (byte)entry.Division;
            var tier = (Tier) (entry.Tier + 1);
            return Tuple.Create(tier, division);
        }

    }
}