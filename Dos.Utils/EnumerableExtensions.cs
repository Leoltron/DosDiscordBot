using System;
using System.Collections.Generic;
using System.Linq;

namespace Dos.Utils
{
    public static class EnumerableExtensions
    {
        private static readonly Random Rand = new();

        public static IEnumerable<T> Repeat<T>(this T element, int repeatAmount)
        {
            for (var i = 0; i < repeatAmount; i++)
                yield return element;
        }

        public static IEnumerable<T> Times<T>(this IEnumerable<T> source, int repeatAmount)
        {
            return source.SelectMany(e => e.Repeat(repeatAmount));
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();


        public static T RandomElement<T>(this IList<T> list) =>
            list.IsEmpty()
                ? throw new ArgumentException($"{nameof(list)} cannot be empty!")
                : list[Rand.Next(list.Count)];

        public static T RandomElementOrDefault<T>(this IList<T> list, T defaultValue = null) where T : class =>
            list.IsEmpty() ? defaultValue : list.RandomElement();

        public static T? RandomElementOrDefault<T>(this IList<T> list) where T : struct =>
            list.IsEmpty() ? (T?) null : list.RandomElement();

        public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> source) where T : struct =>
            source.Cast<T?>();

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<Tuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<ValueTuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);

        public static Dictionary<TKey, TValue> UnionWith<TKey, TValue>(this IDictionary<TKey, TValue> one,
                                                                       IDictionary<TKey, TValue> other) =>
            one.Concat(other).ToDictionary(p => p.Key, p => p.Value);

        public static IEnumerable<T> TakeWhileNotNull<T>(this IEnumerable<T?> source) where T : struct
        {
            foreach (var element in source)
                if (element.HasValue)
                    yield return element.Value;
                else
                    yield break;
        }

        public static IEnumerable<IList<T>> ToChunks<T>(this IEnumerable<T> source, int maxChunkSize)
        {
            if (maxChunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkSize), maxChunkSize, "Expected positive value");

            var list = new List<T>(maxChunkSize);
            foreach (var element in source)
            {
                list.Add(element);
                if (list.Count != maxChunkSize)
                    continue;
                yield return list;
                list = new List<T>(maxChunkSize);
            }

            if (list.Count != 0)
                yield return list;
        }

        public static T RemoveAtAndReturn<T>(this IList<T> list, int index)
        {
            var element = list[index];
            list.RemoveAt(index);
            return element;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
                action(element);
        }

        public static TValue MaxOrDefault<TSource, TValue>(this IEnumerable<TSource> source,
                                                           Func<TSource, TValue> selector,
                                                           TValue defaultValue = default)
            where TValue : IComparable<TValue> =>
            source.Select(selector).MaxOrDefault(defaultValue);

        public static T MaxOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
            where T : IComparable<T>
        {
            var isEmpty = true;
            var max = defaultValue;
            foreach (var element in source)
                if (isEmpty)
                {
                    isEmpty = false;
                    max = element;
                }
                else if (max.CompareTo(element) < 0)
                {
                    max = element;
                }

            return max;
        }

        public static IEnumerable<T> WhereHasValue<T>(this IEnumerable<T?> source) where T : struct =>
            from element in source where element.HasValue select element.Value;

        public static TSource WithMax<TSource, TComparable>(this IEnumerable<TSource> source,
                                                            Func<TSource, TComparable> selector,
                                                            TSource defaultValue = default)
            where TComparable : IComparable<TComparable>
        {
            var firstPassed = false;
            var maxElement = defaultValue;
            var maxValue = default(TComparable);

            foreach (var element in source)
            {
                var value = selector(element);
                if (!firstPassed)
                {
                    firstPassed = true;
                    maxElement = element;
                    maxValue = value;
                    continue;
                }

                if (maxValue.CompareTo(value) < 0)
                {
                    maxElement = element;
                    maxValue = value;
                }
            }

            return maxElement;
        }

        public static List<(T element, int index)> AddShiftIndices<T>(this IList<T> list, int startIndex)
        {
            if(startIndex < 0 || startIndex >= list.Count)
                throw new IndexOutOfRangeException();
            return list.Select((element, i) => (element, (i - startIndex + list.Count) % list.Count)).ToList();
        }
    }
}
