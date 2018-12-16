using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.ChampionMasteryV4;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.LeagueV4;
using MingweiSamuel.Camille.SummonerV4;
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

        public Task<Summoner> GetSummonerByName(string region, string summonerName)
        {
            return _api.SummonerV4.GetBySummonerNameAsync(Region.Get(region), summonerName);
        }

        public Task<MingweiSamuel.Camille.SummonerV3.Summoner> GetSummonerByNameV3(string region, string summonerName)
        {
            return _api.SummonerV3.GetBySummonerNameAsync(Region.Get(region), summonerName);
        }

        public Task<Summoner> GetSummoner(string region, string summonerIdEnc)
        {
            return _api.SummonerV4.GetBySummonerIdAsync(Region.Get(region), summonerIdEnc);
        }

        public Task<MingweiSamuel.Camille.SummonerV3.Summoner> GetSummonerV3(string region, long summonerId)
        {
            return _api.SummonerV3.GetBySummonerIdAsync(Region.Get(region), summonerId);
        }

        // V3 // Not used -- upgrade breaks this abstraction.
//        public async Task<IDictionary<string, Summoner>> GetSummoners(string region, ICollection<string> summonerIdEncs)
//        {
//            var pairs = await Task.WhenAll(
//                summonerIdEncs.Select(async idEnc => new KeyValuePair<string, Summoner>(idEnc, await GetSummoner(region, idEnc))));
//            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
//        }

        // Note: modifies the summoner argument with new SummonerIdEnc if needed.
        public async Task<Summoner> GetSummonerUpgradeToV4(Data.Models.Summoner summoner)
        {
            if (null != summoner.SummonerIdEnc)
            {
                return await GetSummoner(summoner.Region, summoner.SummonerIdEnc);
            }
            // Extra case for converting V3.
            if (null == summoner.SummonerId)
                throw new InvalidOperationException($"Summoner with DB ID {summoner.Id} missing summoner IDs.");
            var summonerDataV3 = await this.GetSummonerV3(summoner.Region, (long) summoner.SummonerId);
            return await this.GetSummonerByName(summoner.Region, summonerDataV3.Name);
        }

        public async Task<Tuple<Tier, Division>> GetRank(string region, string summonerIdEnc)
        {
            var response = await _api.LeagueV4.GetAllLeaguePositionsForSummonerAsync(Region.Get(region), summonerIdEnc);
            return GetRankFromResponse(response);
        }

        public async Task<IDictionary<string, Tuple<Tier, Division>>> GetRanks(string region, ICollection<string> summonerIdEncs)
        {
            var pairs = await Task.WhenAll(
                summonerIdEncs.Select(async idEnc => new KeyValuePair<string, Tuple<Tier, Division>>(idEnc, await GetRank(region, idEnc))));
            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Task<ChampionMastery[]> GetChampionMastery(string region, string summonerIdEnc)
        {
            return _api.ChampionMasteryV4.GetAllChampionMasteriesAsync(Region.Get(region), summonerIdEnc);
        }

        public async Task<IDictionary<string, ChampionMastery[]>> GetChampionMasteries(string region,
            ICollection<string> summonerIdEncs)
        {
            var pairs = await Task.WhenAll(
                summonerIdEncs.Select(async idEnc => new KeyValuePair<string, ChampionMastery[]>(idEnc, await GetChampionMastery(region, idEnc))));
            return pairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static Tuple<Tier, Division> GetRankFromResponse(IEnumerable<LeaguePosition> json)
        {
            var entry = json.FirstOrDefault(e => Queue.RANKED_SOLO_5x5 == e.QueueType);
            if (entry == null)
                return null;
            Division div;
            Enum.TryParse(entry.Rank, out div);
            Tier tier;
            Enum.TryParse(entry.Tier.First().ToString().ToUpperInvariant() + entry.Tier.Substring(1).ToLowerInvariant(), out tier);
            return Tuple.Create(tier, div);
        }

    }
}