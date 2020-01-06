using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.DiscordBot.Util
{
    public static class CardParser
    {
        private static readonly Dictionary<string, CardValue> ShortValues =
            Enum.GetValues(typeof(CardValue))
                .Cast<CardValue>()
                .ToDictionary(
                     v => v == CardValue.Sharp
                         ? "#"
                         : ((int) v).ToString());

        private static readonly Dictionary<string, CardColor> ShortColors =
            Enum.GetValues(typeof(CardColor))
                .Cast<CardColor>()
                .ToDictionary(c => c.ToString().Substring(0, 1).ToLowerInvariant());

        private static readonly Dictionary<string, CardValue> Values =
            Enum.GetValues(typeof(CardValue))
                .Cast<CardValue>()
                .ToDictionary(v => v.ToString().ToLowerInvariant());

        private static readonly Dictionary<string, CardColor> Colors =
            Enum.GetValues(typeof(CardColor))
                .Cast<CardColor>()
                .ToDictionary(v => v.ToString().ToLowerInvariant());

        private static readonly Dictionary<string, CardColor> AllColors;
        private static readonly Dictionary<string, CardValue> AllValues;

        private static readonly Dictionary<string, Card> ShortCards;

        static CardParser()
        {
            Values.Add("uno", CardValue.One);
            Values.Add("dos", CardValue.Two);
            Values.Add("hash", CardValue.Sharp);
            
            ShortValues.Add("d", CardValue.Two);
            ShortValues.Add("h", CardValue.Sharp);
            ShortValues.Add("s", CardValue.Sharp);

            AllColors = ShortColors.UnionWith(Colors);
            AllValues = ShortValues.UnionWith(Values);

            ShortCards = new Dictionary<string, Card>(ShortColors
                                                         .SelectMany(c => ShortValues
                                                                        .SelectMany(v => new[]
                                                                         {
                                                                             new KeyValuePair<string, Card>(
                                                                                 c.Key + v.Key, v.Value.Of(c.Value)),
                                                                             new KeyValuePair<string, Card>(
                                                                                 v.Key + c.Key, v.Value.Of(c.Value))
                                                                         })));
        }

        public static Result<(Card[] matchers, Card target)> ParseMatchCards(string s)
        {
            if (!s.Contains(" on ", StringComparison.InvariantCultureIgnoreCase))
            {
                return Result<(Card[] matchers, Card target)>.Fail(
                    "You supposed to select target using `dos select <card>` or say `dos match <card1> [<card2>] on <centerRowCard>`");
            }

            var split = s.ToLowerInvariant().Split(" on ", 2);
            if (split.Length != 2)
            {
                return Result<(Card[] matchers, Card target)>.Fail(
                    "You supposed to select target using `dos select <card>` or say `dos match <card1> [<card2>] on <centerRowCard>`");
            }

            var matcherCardsResult = ParseCards(split[0]);
            if (!matcherCardsResult.IsSuccess)
            {
                return Result<(Card[] matchers, Card target)>.Fail(matcherCardsResult.Message);
            }

            var targetCardResult = ParseCards(split[1]);
            if (!targetCardResult.IsSuccess)
            {
                return Result<(Card[] matchers, Card target)>.Fail(matcherCardsResult.Message);
            }

            if (targetCardResult.Value.Count != 1)
            {
                return Result<(Card[] matchers, Card target)>.Fail("There must be one target card");
            }

            return (matcherCardsResult.Value.ToArray(), targetCardResult.Value.First()).ToSuccess();
        }

        public static Result<List<Card>> ParseCards(string s) =>
            ParseCards(s.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        public static Result<List<Card>> ParseCards(string[] words)
        {
            var cards = new List<Card>();
            string prevWord = null;
            CardColor? color = null;
            CardValue? value = null;

            foreach (var word in words)
            {
                if (color != null)
                {
                    if (AllValues.TryGetValue(word, out var parsedValue))
                    {
                        cards.Add(parsedValue.Of((CardColor) color));
                        color = null;
                    }
                    else
                    {
                        return Result<List<Card>>.Fail($"Expected a card value after {prevWord}, got \"{word}\"");
                    }
                }
                else if (value != null)
                {
                    if (AllColors.TryGetValue(word, out var parsedColor))
                    {
                        cards.Add(((CardValue) value).Of(parsedColor));
                        value = null;
                    }
                    else
                    {
                        return Result<List<Card>>.Fail($"Expected a card color after {prevWord}, got \"{word}\"");
                    }
                }
                else if (ShortCards.TryGetValue(word, out var card))
                    cards.Add(card);
                else if (AllValues.TryGetValue(word, out var parsedValue))
                {
                    value = parsedValue;
                }
                else if (AllColors.TryGetValue(word, out var parsedColor))
                {
                    color = parsedColor;
                }
                else
                {
                    return Result<List<Card>>.Fail($"Expected a shortened card or color or value, got \"{word}\"");
                }

                prevWord = word;
            }

            return cards.ToSuccess();
        }
    }
}
