using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reddit;

namespace ChampionMains.Pyrobot.Services.Reddit
{
    /// <summary>
    /// Deals with getting and re-getting the access_token for RedditAPI.
    /// </summary>
    public class RedditApiProvider
    {
        private const string AccessTokenUrl = "https://www.reddit.com/api/v1/access_token";
        private readonly string _userAgent;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _userName;
        private readonly string _password;

        private readonly Semaphore _accessTokenSemaphore = new Semaphore(1, 1);

        private string _accessToken;
        private DateTime _accessTokenExpires;
        private RedditAPI _redditApi;

        public RedditApiProvider(string clientId, string clientSecret, string username, string password, string userAgent)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _userName = username;
            _password = password;
            _userAgent = Uri.EscapeDataString(userAgent);
        }

        public async Task<RedditAPI> GetRedditApi()
        {
            await UpdateIfNeeded();
            return _redditApi;
        }

        private async Task UpdateIfNeeded()
        {
            // Suport for LocalFunctions is lacking in Azure, it seems. 2019-05-24.
            // ReSharper disable once ConvertToLocalFunction
#pragma warning disable IDE0039 // Use local function
            Func<bool> needsUpdate = () => string.IsNullOrEmpty(_accessToken) || DateTime.Now >= _accessTokenExpires;
#pragma warning restore IDE0039 // Use local function

            if (needsUpdate())
            {
                // TODO await within semaphore might deadlock?
                _accessTokenSemaphore.WaitOne();
                try
                {
                    if (needsUpdate())
                        await Update();
                }
                finally
                {
                    _accessTokenSemaphore.Release();
                }
            }
        }

        private async Task Update()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(_clientId, _clientSecret)
            }); // TODO REUSE / DI ?

            var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenUrl);
            request.Headers.Add("User-Agent", _userAgent);
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
            _redditApi = new RedditAPI(appId: _clientId, appSecret: _clientSecret, accessToken: _accessToken);
            // Subtract a few minutes as buffer time.
            _accessTokenExpires = DateTime.Now.AddSeconds((double) content["expires_in"]).AddMinutes(-2);
        }
    }
}
