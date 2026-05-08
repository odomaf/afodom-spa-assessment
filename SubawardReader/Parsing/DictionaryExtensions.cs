using System.Collections.Generic;

namespace SubawardReader.Parsing
{
    public static class DictionaryExtensions
    {
        // Returns the value for the given key if present, otherwise returns the provided default value.

        public static TValue TryGetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                // Safe for value types (e.g., decimal), suppresses CS8601
                return defaultValue!;
            if (key != null && dict.TryGetValue(key, out var value))
                return value;
            // Safe for value types (e.g., decimal), suppresses CS8601
            return defaultValue!;
        }

        // Extension for IReadOnlyDictionary (covers SubawardRow.Amounts)
        public static TValue TryGetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                // Safe for value types (e.g., decimal), suppresses CS8601
                return defaultValue!;
            if (key != null && dict.TryGetValue(key, out var value))
                return value;
            // Safe for value types (e.g., decimal), suppresses CS8601
            return defaultValue!;
        }
    }
}
