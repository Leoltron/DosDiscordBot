using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Serilog.Events;

namespace Dos.DiscordBot
{
    public static class Extensions
    {
        private static readonly Random Rand = new Random();

        public static T RandomElement<T>(this IList<T> list) =>
            list.Count == 0
                ? throw new ArgumentException($"{nameof(list)} cannot be empty!")
                : list[Rand.Next(list.Count)];

        public static LogEventLevel ToLogLevel(this LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogEventLevel.Fatal;
                case LogSeverity.Error:
                    return LogEventLevel.Error;
                case LogSeverity.Warning:
                    return LogEventLevel.Warning;
                case LogSeverity.Info:
                    return LogEventLevel.Information;
                case LogSeverity.Verbose:
                    return LogEventLevel.Debug;
                case LogSeverity.Debug:
                    return LogEventLevel.Verbose;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<Tuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<ValueTuple<TKey, TValue>> enumerable) =>
            enumerable.ToDictionary(t => t.Item1, t => t.Item2);
    }
}
