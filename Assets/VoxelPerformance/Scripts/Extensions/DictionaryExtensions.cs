using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrSet<K,T>(this Dictionary<K,T> d, K key, T val)
        {
            if(d.ContainsKey(key)) { d[key] = val; }
            else { d.Add(key, val); }
        }

        public static T GetOrAdd<K, T>(this Dictionary<K,T> d, K key, T val)
        {
            if(d.ContainsKey(key)) { return d[key]; }
            d.Add(key, val);
            return val;
        }

        public static T SafeGet<K, T>(this Dictionary<K,T> d, K key)
        {
            if(d.ContainsKey(key)) { return d[key]; }
            return default(T);
        }
    }
}
