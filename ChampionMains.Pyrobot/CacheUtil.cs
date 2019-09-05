using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace ChampionMains.Pyrobot
{
    public static class CacheUtil
    {
        private static readonly TimeSpan DefaultExpirery = TimeSpan.FromMinutes(2);

        public static async Task<TResult> GetItemAsync<TResult>(string key, Func<Task<TResult>> createItem,
            bool force = false, TimeSpan? expirery = null)
            where TResult : class
        {
            var item = HttpRuntime.Cache[key] as TResult;

            if (force || item == null)
            {
                item = await createItem() ?? item;
                if (item == null)
                    return null;
                HttpRuntime.Cache.Add(key, item, null, DateTime.Now + (expirery ?? DefaultExpirery),
                    Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }
            return item;
        }
    }
}
