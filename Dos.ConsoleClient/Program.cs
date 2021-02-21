using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game;
using Dos.Game.Deck;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.ConsoleClient
{
    internal static class Program
    {
        private static void Main()
        {
            var game = new DosGame(new ShufflingDealer(Decks.Classic), 1, 7);
            var p = game.Players.First();
            while (true)
            {
                var line = Console.ReadLine();
                line = line?.ToLowerInvariant().Trim();
                if (line == "quit")
                    return;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line == "dos cards" || line == "dosc")
                {
                    Console.WriteLine(string.Join(Environment.NewLine, game.PersonalGameTableLines(p)));
                    continue;
                }

                if (line == "dosd")
                {
                    Console.WriteLine(game.EndTurn(p));
                    continue;
                }

                if (line.StartsWith("dos play "))
                {
                    var cardString = line.Substring(9).Trim();
                    Console.WriteLine(
                        CardParser.TryParseShortCard(cardString, out var card)
                            ? game.AddCardToCenterRow(p, card).Message
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
                        if (!lookingForTarget)
                        {
                            if (cardWord == "on")
                            {
                                lookingForTarget = true;
                                continue;
                            }

                            if (CardParser.TryParseShortCard(cardWord, out var card))
                                cards.Add(card);
                        }
                        else
                        {
                            if (CardParser.TryParseShortCard(cardWord, out var card))
                                target = card;

                            break;
                        }

                    if (cards.IsEmpty() || target == null)
                        Console.WriteLine("Failed to parse your matching cards, sorry");
                    else
                        Console.WriteLine(game.MatchCenterRowCard(p, target.Value, cards.ToArray()));
                }
            }
        }
    }
}
