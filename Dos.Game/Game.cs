using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game
{
    public class Game : IGame
    {
        public List<Card> centerRow = new List<Card>(8);
        public List<List<Card>> centerRowAdditional = new List<List<Card>>(8);
        public int CurrentPlayerPenalty;

        public Stack<Card> Deck;
        public Stack<Card> discardPile;

        public List<Card>[] playerHands;
        public int? PlayerWhoDidNotCallDos;

        public GameConfig Config { get; }

        public Game(IDeckGenerator deckGenerator, int playersCount, ushort initialHandSize) : this(
            deckGenerator, playersCount, new GameConfig {InitialHandSize = initialHandSize})
        {
        }

        public Game(IDeckGenerator deckGenerator, int playersCount, GameConfig config)
        {
            Config = config;
            Deck = new Stack<Card>(deckGenerator.Generate());
            discardPile = new Stack<Card>();

            playerHands = new List<Card>[playersCount];
            for (var i = 0; i < playersCount; i++)
            {
                playerHands[i] = new List<Card>(10);
                DealCards(i, config.InitialHandSize, false);
            }

            RefillCenterRow();
            CurrentPlayer = new Random().Next(playersCount);
            CurrentState = new TurnStartState(this);
            CenterRowSizeAtTurnStart = centerRow.Count;
        }

        public GameState CurrentState { get; set; }

        public bool AllowCallouts { get; set; } = true;

        public int CurrentPlayer { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
        public int PlayersCount => playerHands.Length;

        public List<Card> CurrentPlayerHand => playerHands[CurrentPlayer];
        public string CurrentPlayerName => GetPlayerName(CurrentPlayer);

        public int TotalScore => playerHands.SelectMany(h => h).Sum(c => c.Points);


        public List<(string name, int score)> ScoreTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Sum(c => c.Points)))
                      .ToList();

        public List<(string name, int cardsCount)> HandsTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), playerHands[i].Count))
                      .ToList();

        private int CenterRowSizeAtTurnStart { get; set; }
        public int MatchCount { get; set; }

        public Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            CurrentState.MatchCenterRowCard(player, target, cardsToPlay)
                        .DoIfSuccess(_ => PlayerMatchedCard?.Invoke(player, cardsToPlay, target));

        public Result EndTurn(int player) => CurrentState.EndTurn(player);

        public Result Draw(int player) => CurrentState.Draw(player);

        public Result AddCardToCenterRow(int player, Card card) =>
            CurrentState.AddCardToCenterRow(player, card)
                        .DoIfSuccess(_ => PlayerAddedCard?.Invoke(player, card));

        public Result Callout(int caller)
        {
            var playerWhoDidNotCallDos = PlayerWhoDidNotCallDos;
            if (!AllowCallouts || caller == playerWhoDidNotCallDos)
                return Result.Fail();

            return CurrentState.Callout(caller)
                               .DoIfSuccess(_ => CalledOut?.Invoke(caller, playerWhoDidNotCallDos ?? -1))
                               .DoIfFail(_ => FalseCallout?.Invoke(caller));
        }

        public Result CallDos(int caller) => AllowCallouts
            ? CurrentState.CallDos(caller).DoIfSuccess(_ => DosCall?.Invoke(caller))
            : Result.Fail();

        public delegate void OnPlayerSwitched(int nextPlayer, int unmatchedCardsCount);

        public event OnPlayerSwitched PlayerSwitched;
        public event Action<int, Card[]> PlayerReceivedCards;
        public event Action<int, Card[], Card> PlayerMatchedCard;
        public event Action<int, Card> PlayerAddedCard;
        public event Action<int> DosCall;
        public event Action<int> FalseCallout;
        public event Action<int, int> CalledOut;
        public event Action Finished;

        public string GetPlayerName(int id) => PlayerNames.TryGetValue(id, out var name) ? name : "Player " + id;

        public void DealCard(int player, bool checkForDos = true)
        {
            DealCards(player, 1, checkForDos);
        }

        public void DealCards(int player, int amount, bool checkForDos = true)
        {
            if (amount <= 0)
                return;

            var cardsDealt = Enumerable.Range(0, amount)
                                       .Select(_ => DealCardInternal(player, checkForDos))
                                       .TakeWhileNotNull()
                                       .ToArray();

            PlayerReceivedCards?.Invoke(player, cardsDealt);
        }

        private Card? DealCardInternal(int player, bool checkForDos) =>
            DrawCard().DoIfHasValue(card =>
            {
                playerHands[player].Add(card);
                if (checkForDos && player == CurrentPlayer)
                    CheckCurrentPlayerForDos();
            });

        public void CheckCurrentPlayerForDos()
        {
            if (playerHands[CurrentPlayer].Count == 2)
                PlayerWhoDidNotCallDos = CurrentPlayer;
        }

        private Card? DrawCard()
        {
            if (EnsureDeckHasCards())
                return Deck.Pop();
            return null;
        }

        private bool EnsureDeckHasCards()
        {
            if (Deck.Any())
                return true;

            if (discardPile.IsEmpty())
                return false;

            var newDeck = discardPile.ToList();
            newDeck.Shuffle();
            Deck = new Stack<Card>(newDeck);
            discardPile.Clear();
            return true;
        }

        public bool RefillCenterRow()
        {
            var refillNeeded = centerRow.Count < Config.MinCenterRowSize;
            while (centerRow.Count < Config.MinCenterRowSize)
            {
                var card = DrawCard();
                if (card == null)
                    break;
                centerRow.Add(card.Value);
            }

            while (centerRowAdditional.Count < centerRow.Count)
                centerRowAdditional.Add(new List<Card>());
            return refillNeeded;
        }

        public void MoveTurnToNextPlayer()
        {
            var unmatchedCardsCount = CenterRowSizeAtTurnStart - MatchCount;
            if (Config.CenterRowPenalty)
                CurrentPlayerPenalty += unmatchedCardsCount;
            DealCards(CurrentPlayer, CurrentPlayerPenalty);
            CurrentPlayerPenalty = 0;
            CurrentPlayer = (CurrentPlayer + 1) % PlayersCount;
            CenterRowSizeAtTurnStart = centerRow.Count;
            MatchCount = 0;
            PlayerSwitched?.Invoke(CurrentPlayer, unmatchedCardsCount);
        }

        public IEnumerable<string> PersonalGameTableLines(int player)
        {
            foreach (var p in GameTableLines())
                yield return p;

            foreach (var p in GetPlayerHandLines(player))
                yield return p;
        }

        public IEnumerable<string> GetPlayerHandLines(int player)
        {
            var hand = playerHands[player];
            yield return $"Your current hand ({hand.Count} {(hand.Count == 1 ? "card" : "cards")}):";
            yield return "\u200b";
            yield return hand.ToDiscordString();
        }

        public IEnumerable<string> GameTableLines() =>
            centerRow.Select((t, i) => " - " + t +
                                       (centerRowAdditional[i]
                                          .Any()
                                           ? $" with {string.Join(" and ", centerRowAdditional[i])} on top"
                                           : string.Empty));

        public void SetFinished()
        {
            CurrentState = new FinishedGameState(this);
            Finished?.Invoke();
        }
    }
}
