using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Services
{
    public class ValidationService
    {
        public Task<string> GenerateAsync(string principal, int summonerId, string region, string userName)
        {
            var nonce = string.Join(":", principal, summonerId, region, userName).ToLowerInvariant();
            return Task.FromResult(ToHexString(Hash(nonce)));
        }

        public async Task<bool> ValidateAsync(string principal, int summonerId, string region, string userName, string code)
        {
            return string.Equals(code, await GenerateAsync(principal, summonerId, region, userName),
                StringComparison.OrdinalIgnoreCase);
        }

        private static uint Hash(string s)
        {
            return s.Aggregate<char, uint>(5381, (current, c) => (current << 5) + current + c);
        }

        private static string ToHexString(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            return string.Join("", bytes.Select(x => x.ToString("X2")));
        }
    }
}