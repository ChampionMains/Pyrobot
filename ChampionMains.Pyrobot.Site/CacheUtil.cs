using System;
using System.Threading.Tasks;
using System.Web;

namespace ChampionMains.Pyrobot
{
    public static class CacheUtil
    {
        public static TResult GetItem<TResult>(string key, Func<TResult> createItem)
            where TResult : class
        {
            TResult item = HttpRuntime.Cache[key] as TResult;

            if (item == null)
            {
                item = createItem();
                HttpRuntime.Cache[key] = item;
            }
            return item;
        }

        public static async Task<TResult> GetItemAsync<TResult>(string key, Func<Task<TResult>> createItem)
            where TResult : class
        {
            TResult item = HttpRuntime.Cache[key] as TResult;

            if (item == null)
            {
                item = await createItem();
                if (item != null)
                    HttpRuntime.Cache[key] = item;
            }
            return await Task.FromResult(item);
        }
    }
}