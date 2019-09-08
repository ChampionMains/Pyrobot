using System.Collections.Generic;
using System.Linq;

namespace ChampionMains.Pyrobot.Util
{
    public static class ContainsAllExtension
    {
        public static bool ContainsAll<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return !b.Except(a).Any();
        }
    }
}
