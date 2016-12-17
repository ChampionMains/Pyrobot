using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChampionMains.Pyrobot.Data.Enums;
using Newtonsoft.Json.Linq;

namespace ChampionMains.Pyrobot.Services.Riot
{
    public class RiotWebRequester
    {
        private static readonly IEnumerable<string> Regions = RegionUtils.GetRegionStrings();
        private static readonly Dictionary<string, string> RegionPlatforms = Regions.Zip(
            new[] { "BR1", "EUN1", "EUW1", "JP1", "KR", "LA1", "LA2", "NA1", "OC1", "RU", "TR1" }, 
            (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v, StringComparer.OrdinalIgnoreCase);

        private class RateLimitThrottle
        {
            private const string XRateLimitCount = "X-Rate-Limit-Count";

            private readonly IDictionary<TimeSpan, int> _timeSpanLimits;
            private readonly IDictionary<TimeSpan, int> _counts = new Dictionary<TimeSpan, int>();

            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

            private DateTime _lastUpdate = DateTime.MinValue;
            private TimeSpan _retryAfter = TimeSpan.Zero;

            public RateLimitThrottle(string rateLimit)
            {
                _timeSpanLimits = ParseRateLimit(rateLimit);

                if (_timeSpanLimits == null)
                {
                    throw new ArgumentException(nameof(rateLimit) + " string format is invalid");
                }

                foreach (var timeSpan in _timeSpanLimits.Keys)
                {
                    _counts[timeSpan] = 0;
                }
            }

            public void UpdateRateLimit(HttpResponseMessage response)
            {
                _lastUpdate = DateTime.Now;

                if (response.Headers.Contains(XRateLimitCount))
                {
                    var rateLimitHeader = response.Headers.GetValues(XRateLimitCount).First();
                    var newCounts = ParseRateLimit(rateLimitHeader);
                    if  (newCounts != null)
                        foreach (var kvp in newCounts)
                            _counts[kvp.Key] = kvp.Value;
                }

                if (response.Headers.RetryAfter != null)
                {
                    int delay;
                    if (int.TryParse(response.Headers.RetryAfter.ToString().Trim(), out delay))
                        _retryAfter = TimeSpan.FromSeconds(delay);
                }
            }

            private static IDictionary<TimeSpan, int> ParseRateLimit(string rateLimit)
            {
                if (!Regex.IsMatch(rateLimit, @"^\d+:\d+(,\s*\d+:\d+)*$"))
                    return null;

                return rateLimit.Split(',')
                    .Select(str => str.Trim().Split(':'))
                    .ToDictionary(pair => TimeSpan.FromSeconds(int.Parse(pair[1])), pair => int.Parse(pair[0]));
            }

            public async Task Throttle()
            {
                var now = DateTime.Now;
                var waitTime = TimeSpan.Zero;

                // respect x-rate-limit header
                foreach (var kvp in _timeSpanLimits)
                {
                    var timeSpan = kvp.Key;

                    var delay = _lastUpdate + timeSpan - now;
                    if (delay <= waitTime)
                        continue;

                    if (_counts[kvp.Key] < kvp.Value)
                        continue;

                    waitTime = delay;
                }

                // respect retryafter header
                var retryAfterTime = _lastUpdate + _retryAfter - now;
                if (waitTime < retryAfterTime)
                    waitTime = retryAfterTime;

                if (waitTime > TimeSpan.Zero)
                {
                    await Console.Out.WriteLineAsync($"RiotWebRequester rate-limit hit, delaying for {waitTime}");
                    await Task.Delay(waitTime);
                }
            }
        }

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, RateLimitThrottle> _throttlePerRegion;

        public string ApiKey { get; set; }
        public int MaxAttempts { get; set; }
        public TimeSpan RetryInterval { get; set; }

        public RiotWebRequester(string rateLimit)
        {
            _throttlePerRegion = Regions.ToDictionary(region => region,
                region => new RateLimitThrottle(rateLimit), StringComparer.OrdinalIgnoreCase);
        }

        public async Task<JToken> SendRequestAsync(string region, string relativeUri,
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
            return JToken.Parse(await response.Content.ReadAsStringAsync());
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

            var failedRequests = new List<RiotHttpException>();

            while (attempts-- > 0)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);


                var throttler = _throttlePerRegion[region];
                await throttler.Lock.WaitAsync();

                try
                {
                    // do throttling
                    await throttler.Throttle();
                    var response = await _httpClient.SendAsync(request);
                    throttler.UpdateRateLimit(response);


                    switch ((int) response.StatusCode)
                    {
                        case 404:
                        case 200:
                            // 404 and 200 are both considered "success" status codes, with
                            // 404 indicating the request was successful but the entity did not exist (invalid summoner id, for example)
                            return response;

                        case 429:
                        case 500:
                        case 503:
                            // 429 too many requests (rate limtied) ... but we can continue. Throttler should be updated with the rate limit
                            // 500 and 503 indicate an error on Riot's API. If the attempts aren't depleted, we'll requeue and try again.
                            failedRequests.Add(new RiotHttpException(response.StatusCode, $"Retryable request: {requestUri}. Attempt: {attempts + 1}/{MaxAttempts}."));
                            if (attempts > 0)
                            {
                                await Task.Delay(RetryInterval);
                            }
                            break;
                            // 403 blacklisted (temp or permanent) -- shouldn't happen hopefully
                        default:
                            await Console.Error.WriteLineAsync($"{requestUri} failed with status code {response.StatusCode}.");
                            throw new RiotHttpException(response.StatusCode, $"Unimplemented status code response: {response.StatusCode}. Request: {requestUri}.");
                    }
                }
                finally
                {
                    throttler.Lock.Release();
                }
            }
            throw new RiotHttpException($"Failed to communicate with Riot API. Attempts: {MaxAttempts}. Request: {requestUri}.", new AggregateException(failedRequests));
        }
    }
}