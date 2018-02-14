using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

[assembly: InternalsVisibleTo("ChampionMains.Pyrobot.Test")]
namespace ChampionMains.Pyrobot.Services
{
    public class ValidationService
    {
        private readonly byte[] _key;

        public ValidationService(string key)
        {
            _key = HexStringToBytes(key);
        }

        /// <summary>
        /// Generates a unique token for the given combination of identity informations and profileIcon id.
        /// Token is associated with the current time.
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="summonerId"></param>
        /// <param name="region"></param>
        /// <param name="userName"></param>
        /// <param name="profileIcon"></param>
        /// <returns>Unique HMAC token.</returns>
        public string GenerateToken(string principal, long summonerId, string region, string userName, int profileIcon)
        {
            return GenerateTokenInternal(CurrentEpochMillis(), principal, summonerId, region, userName, profileIcon);
        }

        /// <summary>
        /// Validates the token given the particular identity and profileIcon id.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="principal"></param>
        /// <param name="summonerId"></param>
        /// <param name="region"></param>
        /// <param name="userName"></param>
        /// <param name="profileIcon"></param>
        /// <returns></returns>
        public bool ValidateToken(string token, string principal, long summonerId, string region, string userName,
            int profileIcon)
        {
            var colon = token.IndexOf(':');
            if (colon < 0)
                return false;
            if (!long.TryParse(token.Substring(0, colon), out var millis))
                return false;
            var genTime = EpochMillisToDateTime(millis);
            var delta = DateTime.UtcNow - genTime;
            // Tokens must be used within 5 minutes. (And not be from the future).
            if (delta < TimeSpan.FromSeconds(-1) || delta > TimeSpan.FromMinutes(5))
                return false;
            return token.Equals(GenerateTokenInternal(millis, principal, summonerId, region, userName, profileIcon));
        }

        internal string GenerateTokenInternal(long epoch, string principal, long summonerId, string region,
            string userName, int profileIcon)
        {
            var result = epoch + ":" +
                Hash(string.Join(":", epoch, principal, profileIcon, userName, summonerId, region).ToLowerInvariant());
            return result;
        }

        private string Hash(string s)
        {
            // Not sure how stateless ComputeHash is, so we reinstantiate every time.
            return BytesToHexString(new HMACSHA256(_key).ComputeHash(Encoding.UTF8.GetBytes(s)));
        }

        private static byte[] HexStringToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        private static string BytesToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1);

        internal static long CurrentEpochMillis()
        {
            return (long) (DateTime.UtcNow - EPOCH).TotalMilliseconds;
        }

        internal static DateTime EpochMillisToDateTime(long millis)
        {
            return EPOCH + TimeSpan.FromMilliseconds(millis);
        }
    }
}