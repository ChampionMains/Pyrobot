using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace ChampionMains.Pyrobot
{
    public static class CacheUtil
    {
        private static readonly TimeSpan DefaultExpirery = TimeSpan.FromMinutes(2);

        public static TResult GetItem<TResult>(string key, Func<TResult> createItem,
            bool force = false, TimeSpan? expirery = null)
            where TResult : class
        {
            TResult item = HttpRuntime.Cache[key] as TResult;

            if (force || item == null)
            {
                item = createItem();
                if (item == null)
                    return null;
                HttpRuntime.Cache.Add(key, item, null,
                    Cache.NoAbsoluteExpiration, expirery ?? DefaultExpirery, CacheItemPriority.Default, null);
            }
            return item;
        }

        public static async Task<TResult> GetItemAsync<TResult>(string key, Func<Task<TResult>> createItem,
            bool force = false, TimeSpan? expirery = null)
            where TResult : class
        {
            TResult item = HttpRuntime.Cache[key] as TResult;

            if (force || item == null)
            {
                item = await createItem();
                if (item == null)
                    return null;
                HttpRuntime.Cache.Add(key, item, null,
                    Cache.NoAbsoluteExpiration, expirery ?? DefaultExpirery, CacheItemPriority.Default, null);
            }
            return item;
        }
    }
}