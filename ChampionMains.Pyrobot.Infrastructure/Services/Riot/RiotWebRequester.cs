using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Services.Riot
{
    public class RiotWebRequester
    {
        private static readonly string[] Regions = { "BR", "EUNE", "EUW", "JP", "KR", "LAN", "LAS", "NA", "OCE", "RU", "TR" };
        private static readonly Dictionary<string, string> RegionPlatforms = Regions.Zip(
            new string[] { "BR1", "EUN1", "EUW1", "JP1", "KR", "LA1", "LA2", "NA1", "OC1", "RU", "TR1" }, 
            (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v, StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan RatePeriodDuration = TimeSpan.FromSeconds(10);

        private class ThrottleState
        {
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
            public int NumRequests;
            public DateTime RatePeriodStart = DateTime.MinValue;
        }

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, ThrottleState> _throttleStatesForRegions;

        public string ApiKey { get; set; }
        public int MaxAttempts { get; set; }
        public int MaxRequestsPer10Seconds { get; set; }
        public TimeSpan RetryInterval { get; set; }

        public RiotWebRequester()
        {
            _throttleStatesForRegions = Regions.ToDictionary(region => region,
                region => new ThrottleState(), StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> SendRequestAsync(string region, string relativeUri,
            IEnumerable<KeyValuePair<string, string>> parameters = null, string innerUri="api/lol", bool usePlatform = false)
        {
            var response = await SendRequestInternalAsync(region, innerUri, relativeUri, parameters, usePlatform);
            // not found is a successful status code, indicating that the request was successful
            // but the entity did not exist (invalid summoner id, etc).
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static string GetRequestQueryString(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            if (pairs == null) return "";
            var query = pairs.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
            return string.Join("&", query);
        }


        private static string GetRegionPlatform(string region)
        {
            string platform;
            return RegionPlatforms.TryGetValue(region, out platform) ? platform : null;
        }

        private string GetRequestUri(string region, string innerUri, string relativeUri,
            IEnumerable<KeyValuePair<string, string>> parameters, bool usePlatform)
        {
            var queryString = GetRequestQueryString(parameters);

            if (!string.IsNullOrEmpty(queryString))
            {
                queryString += "&";
            }

            queryString += $"api_key={ApiKey}";

            region = region.ToLowerInvariant();

            return
                $"https://{region}.api.pvp.net/{innerUri}/{(usePlatform ? GetRegionPlatform(region) : region)}/{relativeUri}?{queryString}";
        } 

        

        private async Task<HttpResponseMessage> SendRequestInternalAsync(string region, string innerUri, string relativeUri,
            IEnumerable<KeyValuePair<string, string>> parameters, bool usePlatform)
        {
            var requestUri = GetRequestUri(region, innerUri, relativeUri, parameters, usePlatform);
            var attempts = MaxAttempts;
            while (attempts-- > 0)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                await EnforceRateLimitAsync(region);
                var response = await _httpClient.SendAsync(request);
                
                switch ((int) response.StatusCode)
                {
                    case 404:
                    case 200:
                        // 404 and 200 are both considered "success" status codes, with
                        // 404 indicating the request was successful but the entity did not exist (invalid summoner id, for example)
                        return response;

                    case 500:
                    case 503:
                        // 500 and 503 indicate an error on Riot's API. If the attempts aren't depleted, we'll requeue and try again.
                        if (attempts > 0)
                        {
                            await Task.Delay(RetryInterval);
                            continue;
                        }
                        break;
                }
                throw new RiotHttpException(response.StatusCode, requestUri);
            }
            throw new InvalidOperationException();
        }

        private async Task EnforceRateLimitAsync(string region)
        {
            var state = _throttleStatesForRegions[region];
            await state.Lock.WaitAsync();
            try
            {
                // The duration since the rate period began.
                var runDuration = DateTime.Now - state.RatePeriodStart;

                if (runDuration >= RatePeriodDuration)
                {
                    state.NumRequests = 0;
                    state.RatePeriodStart = DateTime.Now;
                }

                if (++state.NumRequests > MaxRequestsPer10Seconds)
                {
                    var timeToWait = RatePeriodDuration - runDuration;
                    await Task.Delay(timeToWait.TotalSeconds < 1 ? TimeSpan.FromSeconds(1) : timeToWait);
                    state.NumRequests = 1;
                }
            }
            finally
            {
                state.Lock.Release();
            }
        }
    }
}