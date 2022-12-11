using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    /// <summary>
    /// Credit to BlockBuilder57 for this incredibly useful extension
    /// https://github.com/BlockBuilder57/LSIIC/blob/527927cb921c360d9c158008e24bdeaf2059440e/LSIIC/LSIIC.VirtualObjectsInjector/VirtualObjectsInjectorPlugin.cs#L146
    /// </summary>
    public static class DictionaryExtensions
    {
        public static TValue CreateValueIfNewKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new TValue());
            return dictionary[key];
        }

        public static bool AddIfUnique<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }
                
            return false;
        }
    }

    public static class ListExtensions
    {
        public static bool AddIfUnique<T>(this List<T> list, T value) where T : new()
        {
            if (!list.Contains(value))
            {
                list.Add(value);
                return true;
            }

            return false;
        }
    }

    public static class IEnumerableExtensions
    {
        public static string AsJoinedString(this IEnumerable<string> enumerable, string separator = ", ")
        {
            return string.Join(separator, enumerable.ToArray());
        }
        
        public static string AsJoinedString<T>(this IEnumerable<T> enumerable, Func<T, string> selector, string separator = ", ")
        {
            return string.Join(separator, enumerable.Select(selector).ToArray());
        }
    }
}
