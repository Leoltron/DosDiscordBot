using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using Dos.Utils;

namespace Dos.Game
{
    public class Game : IGame
    {
        public List<Card> centerRow = new List<Card>(8);
        public List<List<Card>> centerRowAdditional = new List<List<Card>>(8);
        public bool CurrentPlayerDidNotCallDos;
        public int CurrentPlayerPenalty;

        public Stack<Card> Deck;
        public Stack<Card> discardPile;

        public List<Card>[] playerHands;

        public GameState CurrentState { get; set; }

        public int CalloutPenalty { get; set; }
        public int FalseCalloutPenalty { get; set; }

        public int CurrentPlayer { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
        public int PlayersCount => playerHands.Length;

        public List<Card> CurrentPlayerHand => playerHands[CurrentPlayer];
        public string CurrentPlayerName => GetPlayerName(CurrentPlayer);

        public event Action<int> PlayerSwitch;
        public event Action<int, Card[]> PlayerReceivedCards;

        public Game(IDeckGenerator deckGenerator, int playersCount, int initialHandSize)
        {
            Deck = new Stack<Card>(deckGenerator.Generate());
            discardPile = new Stack<Card>();

            playerHands = new List<Card>[playersCount];
            for (var i = 0; i < playersCount; i++)
            {
                playerHands[i] = new List<Card>(10);
                DealCards(i, initialHandSize);
            }

            EnsureCenterRowIsValid();
            CurrentPlayer = new Random().Next(playersCount);
            CurrentState = new MatchingCenterRowState(this);
        }

        public List<(string name, int score)> ScoreTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Sum(c => c.Points)))
                      .ToList();

        public List<(string name, int cardsCount)> HandsTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Count))
                      .ToList();

        public Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            CurrentState.MatchCenterRowCard(player, target, cardsToPlay);

        public Result FinishMatching(int player) => CurrentState.FinishMatching(player);

        public Result Draw(int player) => CurrentState.Draw(player);

        public Result AddCardToCenterRow(int player, Card card) =>
            CurrentState.AddCardToCenterRow(player, card);

        public Result Callout(int caller) => CurrentState.Callout(caller);

        public Result CallDos(int caller) => CurrentState.CallDos(caller);

        public string GetPlayerName(int id) => PlayerNames.TryGetValue(id, out var name) ? name : "Player " + id;


        public void DealCard(int player)
        {
            DealCards(player, 1);
        }

        public void DealCards(int player, int amount)
        {
            if (amount <= 0) return;

            var cardsDealt = new Card[amount];
            for (var i = 0; i < amount; i++) cardsDealt[i] = DealCardInternal(player);

            PlayerReceivedCards?.Invoke(player, cardsDealt);
        }

        private Card DealCardInternal(int player)
        {
            var card = DrawCard();
            playerHands[player].Add(card);

            if (player == CurrentPlayer) CheckCurrentPlayerForDos();

            return card;
        }

        public void CheckCurrentPlayerForDos()
        {
            if (playerHands[CurrentPlayer].Count == 2) CurrentPlayerDidNotCallDos = true;
        }

        public Card DrawCard()
        {
            EnsureDeckHasCards();
            return Deck.Pop();
        }

        public void EnsureDeckHasCards()
        {
            if (Deck.Any()) return;
            var newDeck = discardPile.ToList();
            newDeck.Shuffle();
            Deck = new Stack<Card>(newDeck);
            discardPile.Clear();
        }

        public void EnsureCenterRowIsValid()
        {
            while (centerRow.Count < 2) centerRow.Add(DrawCard());

            while (centerRowAdditional.Count < centerRow.Count) centerRowAdditional.Add(new List<Card>());
        }

        public void MoveTurnToNextPlayer()
        {
            DealCards(CurrentPlayer, CurrentPlayerPenalty);
            CurrentPlayerPenalty = 0;
            CurrentPlayerDidNotCallDos = false;
            CurrentPlayer = (CurrentPlayer + 1) % PlayersCount;
            PlayerSwitch?.Invoke(CurrentPlayer);
        }

        public IEnumerable<string> PersonalGameTableLines(int player)
        {
            foreach (var p in GameTableLines()) yield return p;

            foreach (var p in GetPlayerHandLines(player)) yield return p;
        }

        public IEnumerable<string> GetPlayerHandLines(int player)
        {
            var hand = playerHands[player];
            yield return $"Your current hand ({hand.Count} {(hand.Count == 1 ? "card" : "cards")}):";
            yield return "\u200b";
            yield return hand.ToDiscordString();
        }

        public IEnumerable<string> GameTableLines()
        {
            yield return "**Center Row:**";
            for (var i = 0; i < centerRow.Count; i++)
            {
                yield return " - " + centerRow[i] +
                             (centerRowAdditional[i]
                                .Any()
                                 ? $" with {string.Join(" and ", centerRowAdditional[i])} on top"
                                 : string.Empty);
            }
        }
    }
}
