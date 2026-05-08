// Suppress CS8601 (possible null reference assignment) warnings for generic default(TValue) usage.
// This is safe for value types (e.g., decimal) and expected for reference types unless a non-null default is provided.
#pragma warning disable CS8601
using System.Collections.Generic;

namespace SubawardReader.Parsing
{
    public static class DictionaryExtensions
    {
        // Returns the value for the given key if present, otherwise returns the provided default value.

        public static TValue TryGetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                return defaultValue;
            if (key != null && dict.TryGetValue(key, out var value))
                return value;
            return defaultValue;
        }

        // Extension for IReadOnlyDictionary (covers SubawardRow.Amounts)
        public static TValue TryGetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                return defaultValue;
            if (key != null && dict.TryGetValue(key, out var value))
                return value;
            return defaultValue;
        }
#pragma warning restore CS8601
    }
}
