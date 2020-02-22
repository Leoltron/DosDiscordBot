using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game
{
    public class Game : IGame
    {
        public delegate void OnPlayerSwitched(int nextPlayer, int unmatchedCardsCount);

        public List<Card> CenterRow = new List<Card>(8);
        public List<List<Card>> CenterRowAdditional = new List<List<Card>>(8);
        public int CurrentPlayerPenalty;

        public List<Card>[] PlayerHands;
        public int? PlayerWhoDidNotCallDos;
        public readonly Dealer Dealer;

        public Game(Dealer dealer, int playersCount, ushort initialHandSize) : this(
            dealer, playersCount, new GameConfig {InitialHandSize = initialHandSize})
        {
        }

        public Game(Dealer dealer, int playersCount, GameConfig config)
        {
            this.Dealer = dealer;
            Config = config;

            PlayerHands = new List<Card>[playersCount];
            for (var i = 0; i < playersCount; i++)
            {
                PlayerHands[i] = new List<Card>(10);
                DealCards(i, config.InitialHandSize, false);
            }

            RefillCenterRow();
            CurrentPlayer = new Random().Next(playersCount);
            CurrentState = new TurnStartState(this);
            CenterRowSizeAtTurnStart = CenterRow.Count;
        }

        public GameConfig Config { get; }

        public GameState CurrentState { get; set; }

        public bool AllowCallouts { get; set; } = true;

        public int CurrentPlayer { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
        public int PlayersCount => PlayerHands.Length;

        public List<Card> CurrentPlayerHand => PlayerHands[CurrentPlayer];
        public string CurrentPlayerName => GetPlayerName(CurrentPlayer);

        public int TotalScore => PlayerHands.SelectMany(h => h).Sum(c => c.Points);


        public List<(string name, int score)> ScoreTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), PlayerHands[i].Sum(c => c.Points)))
                      .ToList();

        public List<(string name, int cardsCount)> HandsTable =>
            Enumerable.Range(0, PlayersCount)
                      .Select(i => (GetPlayerName(i), PlayerHands[i].Count))
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
                PlayerHands[player].Add(card);
                if (checkForDos && player == CurrentPlayer)
                    CheckCurrentPlayerForDos();
            });

        public void CheckCurrentPlayerForDos()
        {
            if (PlayerHands[CurrentPlayer].Count == 2)
                PlayerWhoDidNotCallDos = CurrentPlayer;
        }

        private Card? DrawCard()
        {
            if (Dealer.CanDealCards)
                return Dealer.DealCard();
            return null;
        }

        public bool RefillCenterRow()
        {
            var refillNeeded = CenterRow.Count < Config.MinCenterRowSize;
            while (CenterRow.Count < Config.MinCenterRowSize)
            {
                var card = DrawCard();
                if (card == null)
                    break;
                CenterRow.Add(card.Value);
            }

            while (CenterRowAdditional.Count < CenterRow.Count)
                CenterRowAdditional.Add(new List<Card>());
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
            CenterRowSizeAtTurnStart = CenterRow.Count;
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
            var hand = PlayerHands[player];
            yield return $"Your current hand ({hand.Count} {(hand.Count == 1 ? "card" : "cards")}):";
            yield return "\u200b";
            yield return hand.ToDiscordString();
        }

        public IEnumerable<string> GameTableLines() =>
            CenterRow.Select((t, i) => " - " + t +
                                       (CenterRowAdditional[i]
                                          .Any()
                                           ? $" with {string.Join(" and ", CenterRowAdditional[i])} on top"
                                           : string.Empty));

        public void SetFinished()
        {
            CurrentState = new FinishedGameState(this);
            Finished?.Invoke();
        }
    }
}
