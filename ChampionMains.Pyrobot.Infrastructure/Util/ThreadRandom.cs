using System;

namespace ChampionMains.Pyrobot.Util
{
    public class ThreadRandom
    {
        private static readonly Random Global = new Random();
        [ThreadStatic]
        private static Random Local;

        public static int Next()
        {
            if (Local == null)
            {
                lock (Global)
                {
                    if (Local == null)
                    {
                        var seed = Global.Next();
                        Local = new Random(seed);
                    }
                }
            }
            return Local.Next();
        }

        public static int NextExcluding(int maxExclusive, int excluded)
        {
            if (excluded >= maxExclusive)
                return Next() % maxExclusive;
            var val = Next() % (maxExclusive - 1);
            if (val >= excluded)
                val++;
            return val;
        }
    }
}
