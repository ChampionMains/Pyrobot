using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Services
{
    public class ValidationService
    {
        public Task<string> GenerateAsync(string principal, long summonerId, string region, string userName)
        {
            var nonce = string.Join(":", principal, summonerId, region, userName).ToLowerInvariant();
            return Task.FromResult(HashToString(Hash(nonce)));
        }

        public async Task<bool> ValidateAsync(string principal, long summonerId, string region, string userName, string code)
        {
            return string.Equals(code, await GenerateAsync(principal, summonerId, region, userName),
                StringComparison.OrdinalIgnoreCase);
        }

        private static uint Hash(string s)
        {
            return s.Aggregate<char, uint>(5381, (current, c) => (current << 5) + current + c);
        }

        private static string HashToString(uint value)
        {
            if (value < 10000)
                value += 10230;
            return value.ToString();
        }
    }
}