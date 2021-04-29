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
    public static class DictionaryExtension
    {
        public static TValue AddOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new TValue());
            return dictionary[key];
        }
    }
}
