using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dos.Game.Deck;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.ConsoleClient
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var game = new Game.Game(new ShuffledDeckGenerator(Decks.Classic), 1, 7);
            while (true)
            {
                var line = Console.ReadLine();
                line = line?.ToLowerInvariant().Trim();
                if (line == "quit")
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line == "dos cards" || line == "dosc")
                {
                    Console.WriteLine(string.Join(Environment.NewLine, game.PersonalGameTableLines(0)));
                    continue;
                }

                if (line == "dosd")
                {
                    Console.WriteLine(game.EndTurn(0));
                    continue;
                }

                if (line.StartsWith("dos play "))
                {
                    var cardString = line.Substring(9).Trim();
                    Console.WriteLine(
                        CardParser.TryParse(cardString, out var card)
                            ? game.AddCardToCenterRow(0, card).Message
                            : $"Did not recognise the card \"{cardString}\", expected something like b9");
                    continue;
                }

                if (line.StartsWith("dos match "))
                {
                    var cardWords = line.Substring(10).Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    var cards = new List<Card>();
                    var target = (Card?) null;
                    var lookingForTarget = false;
                    foreach (var cardWord in cardWords)
                    {
                        if (!lookingForTarget)
                        {
                            if (cardWord == "on")
                            {
                                lookingForTarget = true;
                                continue;
                            }

                            if (CardParser.TryParse(cardWord, out var card))
                            {
                                cards.Add(card);
                            }
                        }
                        else
                        {
                            if (CardParser.TryParse(cardWord, out var card))
                            {
                                target = card;
                            }

                            break;
                        }
                    }

                    if (cards.IsEmpty() || target == null)
                    {
                        Console.WriteLine("Failed to parse your matching cards, sorry");
                    }
                    else
                    {
                        Console.WriteLine(game.MatchCenterRowCard(0, target.Value, cards.ToArray()));
                    }
                }
            }
        }
    }

    public static class CardParser
    {
        private const string ValueShort = "(?<value>[1-9ds#]|10)";
        private const string ColorShort = "(?<color>w|g|b|y|r)";

        public static readonly Regex shortCardRegex =
            new Regex($"^(?:(?:{ColorShort}{ValueShort})|(?:{ValueShort}{ColorShort}))$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParse(string cardString, out Card card)
        {
            card = Card.WildDos;
            var match = shortCardRegex.Match(cardString);
            if (!match.Success)
            {
                return false;
            }

            card = ParseValue(match.Groups["value"].Value).Of(ParseColor(match.Groups["color"].Value));
            return true;
        }

        private static CardValue ParseValue(string valueString)
        {
            if (!TryParseValue(valueString, out var value))
            {
                throw new ArgumentOutOfRangeException($"Unknown value \"{valueString}\"");
            }

            return value;
        }

        private static bool TryParseValue(string valueString, out CardValue value)
        {
            value = CardValue.Sharp;
            switch (valueString.ToLowerInvariant().Trim())
            {
                case "1":
                case "one":
                    value = CardValue.One;
                    return true;
                case "2":
                case "d":
                case "two":
                case "dos":
                    value = CardValue.Two;
                    return true;
                case "3":
                case "three":
                    value = CardValue.Three;
                    return true;
                case "4":
                case "four":
                    value = CardValue.Four;
                    return true;
                case "5":
                case "five":
                    value = CardValue.Five;
                    return true;
                case "6":
                case "six":
                    value = CardValue.Six;
                    return true;
                case "7":
                case "seven":
                    value = CardValue.Seven;
                    return true;
                case "8":
                case "eight":
                    value = CardValue.Eight;
                    return true;
                case "9":
                case "nine":
                    value = CardValue.Nine;
                    return true;
                case "10":
                case "ten":
                    value = CardValue.Ten;
                    return true;
                case "#":
                case "s":
                case "h":
                case "hash":
                case "sharp":
                    value = CardValue.Sharp;
                    return true;
                default:
                    return false;
            }
        }

        private static CardColor ParseColor(string colorString)
        {
            if (!TryParseColor(colorString, out var color))
            {
                throw new ArgumentOutOfRangeException($"Unknown color \"{colorString}\"");
            }

            return color;
        }

        private static bool TryParseColor(string colorString, out CardColor color)
        {
            color = CardColor.Wild;
            switch (colorString.ToLowerInvariant())
            {
                case "w":
                case "wild":
                    color = CardColor.Wild;
                    return true;
                case "r":
                case "red":
                    color = CardColor.Red;
                    return true;
                case "b":
                case "blue":
                    color = CardColor.Blue;
                    return true;
                case "y":
                case "yellow":
                    color = CardColor.Yellow;
                    return true;
                case "g":
                case "green":
                    color = CardColor.Green;
                    return true;
                default:
                    return false;
            }
        }
    }
}
