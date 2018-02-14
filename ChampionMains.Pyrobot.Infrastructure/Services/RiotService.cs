using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.ChampionMastery;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.League;
using MingweiSamuel.Camille.Summoner;
using Tier = ChampionMains.Pyrobot.Data.Enums.Tier;

namespace ChampionMains.Pyrobot.Services
{
    public class RiotService
    {
        private readonly RiotApi _api;

        public RiotService(RiotApi api)
        {
            _api = api;
        }

        public Task<Summoner> GetSummoner(string region, string summonerName)
        {
            return _api.Summoner.GetBySummonerNameAsync(Region.Get(region), summonerName);
        }

        public Task<Summoner> GetSummoner(string region, long summonerId)
        {
            return _api.Summoner.GetBySummonerIdAsync(Region.Get(region), summonerId);
        }

        public async Task<IDictionary<long, Summoner>> GetSummoners(string region, ICollection<long> summonerIds)
        {
            var pairs = await Task.WhenAll(
                summonerIds.Select(async id => new KeyValuePair<long, Summoner>(id, await GetSummoner(region, id))));
            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Tuple<Tier, Division>> GetRank(string region, long summonerId)
        {
            var response = await _api.League.GetAllLeaguePositionsForSummonerAsync(Region.Get(region), summonerId);
            return GetRankFromResponse(response);
        }

        public async Task<IDictionary<long, Tuple<Tier, Division>>> GetRanks(string region, ICollection<long> summonerIds)
        {
            var pairs = await Task.WhenAll(
                summonerIds.Select(async id => new KeyValuePair<long, Tuple<Tier, Division>>(id, await GetRank(region, id))));
            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Task<ChampionMastery[]> GetChampionMastery(string region, long summonerId)
        {
            return _api.ChampionMastery.GetAllChampionMasteriesAsync(Region.Get(region), summonerId);
        }

        public async Task<IDictionary<long, ChampionMastery[]>> GetChampionMasteries(string region,
            ICollection<long> summonerIds)
        {
            var pairs = await Task.WhenAll(
                summonerIds.Select(async id => new KeyValuePair<long, ChampionMastery[]>(id, await GetChampionMastery(region, id))));
            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static Tuple<Tier, Division> GetRankFromResponse(IEnumerable<LeaguePosition> json)
        {
            var entry = json.FirstOrDefault(e => Queue.RANKED_SOLO_5x5 == e.QueueType);
            if (entry == null)
                return null;
            Enum.TryParse(entry.Rank, out Division div);
            Enum.TryParse(entry.Tier.First().ToString().ToUpperInvariant() + entry.Tier.Substring(1).ToLowerInvariant(), out Tier tier);
            return Tuple.Create(tier, div);
        }

    }
}