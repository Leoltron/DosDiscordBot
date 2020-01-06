using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Dos.Utils
{
    public static class EnumerableExtensions
    {
        private static readonly Random Rand = new Random();

        public static IEnumerable<T> Repeat<T>(this T element, int repeatAmount)
        {
            for (var i = 0; i < repeatAmount; i++)
                yield return element;
        }

        public static IEnumerable<T> Times<T>(this IEnumerable<T> source, int repeatAmount)
        {
            return source.SelectMany(e => e.Repeat(repeatAmount));
        }

        public static void ShuffleFast<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            var n = list.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do
                {
                    provider.GetBytes(box);
                } while (!(box[0] < n * (byte.MaxValue / n)));

                var k = box[0] % n;
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();


        public static T RandomElement<T>(this IList<T> list) =>
            list.Count == 0
                ? throw new ArgumentException($"{nameof(list)} cannot be empty!")
                : list[Rand.Next(list.Count)];


        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<Tuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<ValueTuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);

        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        public static Dictionary<TKey, TValue> UnionWith<TKey, TValue>(this IDictionary<TKey, TValue> one,
                                                                       IDictionary<TKey, TValue> other) =>
            new Dictionary<TKey, TValue>(one.Concat(other));
    }
}
