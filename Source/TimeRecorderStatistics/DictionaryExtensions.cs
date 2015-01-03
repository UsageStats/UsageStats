namespace TimeRecorderStatistics
{
    using System;
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, Func<TValue> activator)
        {
            TValue v;
            if (!d.TryGetValue(key, out v))
            {
                v = activator();
                d[key] = v;
            }

            return v;
        }
    }
}