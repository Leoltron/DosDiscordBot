using System;
using System.Collections.Generic;

namespace Dos.Utils
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        public static string Pluralize(this int i, string one, string many) => $"{i} {(i == 1 ? one : many)}";
        public static string PluralizedString(this int i, string one, string many) => i == 1 ? one : many;

        public static string Join(this string separator, IEnumerable<string> values) => string.Join(separator, values);

        public static string Replace(this string s, char[] charsToReplace) =>
            string.Empty.Join(s.Split(charsToReplace, StringSplitOptions.RemoveEmptyEntries));
    }
}
