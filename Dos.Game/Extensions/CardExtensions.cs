using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Match;
using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class CardExtensions
    {
        public static readonly CardColor[] AllColors =
            {CardColor.Blue, CardColor.Green, CardColor.Red, CardColor.Yellow};

        public static readonly CardValue[] LowerNumbers =
        {
            CardValue.One, CardValue.Two, CardValue.Three, CardValue.Four, CardValue.Five
        };

        public static readonly CardValue[] HigherNumbers =
        {
            CardValue.Six, CardValue.Seven, CardValue.Eight, CardValue.Nine, CardValue.Ten
        };

        public static readonly CardValue[] AllNumbers = LowerNumbers.Concat(HigherNumbers).ToArray();


        public static Card Of(this CardValue value, CardColor color) => new Card(color, value);

        public static Card Of(this int value, CardColor color) => new Card(color, (CardValue) value);

        public static IEnumerable<Card> OfAllColors(this CardValue value)
        {
            return AllColors.Select(c => value.Of(c));
        }

        public static IEnumerable<Card> OfAllColors(this IEnumerable<CardValue> values)
        {
            return values.SelectMany(v => v.OfAllColors());
        }

        public static IEnumerable<Card> OfAllColors(this IEnumerable<int> values)
        {
            return values.SelectMany(v => ((CardValue) v).OfAllColors());
        }

        public static MatchType MatchWith(this Card target, Card[] matchers)
        {
            return matchers.Length switch
            {
                1 => target.MatchWith(matchers[0]),
                2 => target.MatchWith(matchers[0], matchers[1]),
                _ => MatchType.NoMatch
            };
        }

        public static MatchType MatchWith(this Card target, Card matcher)
        {
            if (target.Value.Matches(matcher.Value))
                return target.Color.Matches(matcher.Color) ? MatchType.SingleColorMatch : MatchType.SingleMatch;

            return MatchType.NoMatch;
        }

        public static MatchType MatchWith(this Card target, Card first, Card second)
        {
            if (target.Value.Matches(first.Value, second.Value))
                return target.Color.Matches(first.Color) && target.Color.Matches(second.Color)
                    ? MatchType.DoubleColorMatch
                    : MatchType.DoubleMatch;

            return MatchType.NoMatch;
        }

        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> sublist) where T : IEquatable<T>
        {
            var elementsCount = source
                               .GroupBy(e => e)
                               .ToDictionary(g => g.Key, g => g.Count());
            foreach (var element in sublist)
            {
                if (!elementsCount.TryGetValue(element, out var c) || c <= 0) return false;

                elementsCount[element] = --c;
            }

            return true;
        }

        public static IEnumerable<Card> OrderByColorAndValue(this IEnumerable<Card> cards)
        {
            return cards.OrderBy(c => c.Color).ThenBy(c => c.Value);
        }

        public static string ToDiscordString(this IEnumerable<Card> cards) =>
            $"**{string.Join(" | ", cards.OrderByColorAndValue())}**";
        public static string ToLogString(this IEnumerable<Card> cards) =>
            $"{string.Join(", ", cards.OrderByColorAndValue())}";

        public static string Join(this IEnumerable<Card> cards) => $"{string.Join(",", cards.OrderByColorAndValue())}";
    }
}
