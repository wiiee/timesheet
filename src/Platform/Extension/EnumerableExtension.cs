namespace Platform.Extension
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtension
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null ? true : !source.Any();
        }

        public static List<T> ToList<T>(this IEnumerable<List<T>> source)
        {
            if(source == null || !source.Any())
            {
                return new List<T>();
            }

            var result = new List<T>();

            foreach(var item in source)
            {
                result.AddRange(item);
            }

            return result;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> source)
        {
            if (source == null || !source.Any())
            {
                return new Dictionary<TKey, TValue>();
            }

            return source.SelectMany(o => o).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
