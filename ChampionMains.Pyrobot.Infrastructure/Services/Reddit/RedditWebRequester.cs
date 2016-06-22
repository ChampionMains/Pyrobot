using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ChampionMains.Pyrobot.Services.Reddit
{
    public class RedditWebRequester
    {
        private const string AccessTokenUrl = "https://www.reddit.com/api/v1/access_token";
        private static readonly string UserAgent = Uri.EscapeDataString("aspnet:hallam.redditrankedflairs:v4.0.0 (by /u/kivinkujata)");
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _userName;
        private readonly string _password;

        private readonly Semaphore _accessTokenSemaphore = new Semaphore(1, 1);
        private readonly Semaphore _requestSemaphore = new Semaphore(1, 1);

        private string _accessToken;
        private DateTime _accessTokenExpires;

        private DateTime _ratePeriodStart;
        private int _numRequests;

        public RedditWebRequester(string clientId, string clientSecret, string username, string password)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _userName = username;
            _password = password;
        }

        public async Task<JToken> GetAsync(string uri, IEnumerable<KeyValuePair<string, string>> parameters = null)
        {
            _requestSemaphore.WaitOne();
            try
            {
                await EnsureRateLimit();
                await EnsureAccessToken();
                uri += ToQueryString(parameters);
                var request = CreateRequestMessage(HttpMethod.Get, uri);
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return JToken.Parse(await response.Content.ReadAsStringAsync());
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        public async Task<JToken> PostAsync(string uri, IEnumerable<KeyValuePair<string, string>> parameters = null)
        {
            _requestSemaphore.WaitOne();
            try
            {
                await EnsureRateLimit();
                await EnsureAccessToken();
                var request = CreateRequestMessage(HttpMethod.Post, uri);
                if (parameters != null)
                {
                    request.Content = new FormUrlEncodedContent(parameters);
                }
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return JToken.Parse(await response.Content.ReadAsStringAsync());
            }
            finally
            {
                _requestSemaphore.Release();
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string uri)
        {
            var message = new HttpRequestMessage(method, uri);
            message.Headers.Add("User-Agent", UserAgent);
            message.Headers.Add("Authorization", "bearer " + _accessToken);
            return message;
        }

        private async Task EnsureAccessToken()
        {
            _accessTokenSemaphore.WaitOne();
            try
            {
                var cutoff = DateTime.Now.AddMinutes(1);
                if (string.IsNullOrEmpty(_accessToken) || DateTime.Now < cutoff)
                {
                    await GetAccessToken();
                }
            }
            finally
            {
                _accessTokenSemaphore.Release();
            }
        }

        private async Task EnsureRateLimit()
        {
            var runDuration = DateTime.Now - _ratePeriodStart;

            if (runDuration > TimeSpan.FromMinutes(1))
            {
                _ratePeriodStart = DateTime.Now;
                _numRequests = 0;
            }

            if (++_numRequests > 60)
            {
                var timeToWait = TimeSpan.FromMinutes(1) - runDuration;
                await Task.Delay(timeToWait.TotalSeconds < 1 ? TimeSpan.FromSeconds(1) : timeToWait);
                _numRequests = 1;
            }
        }

        private async Task GetAccessToken()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(_clientId, _clientSecret)
            });
            var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenUrl);
            request.Headers.Add("User-Agent", UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", _userName),
                new KeyValuePair<string, string>("password", _password)  
            });
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
            _accessToken = (string) content["access_token"];
            _accessTokenExpires = DateTime.Now.AddSeconds((double) content["expires_in"]);
        }

        

        private static string ToQueryString<TValue>(IEnumerable<KeyValuePair<string, TValue>> values)
        {
            if (values == null) return "";
            var pairs = values.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value.ToString())}");
            return "?" + string.Join("&", pairs);
        }
    }
}