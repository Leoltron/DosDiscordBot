using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.State;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game
{
    public class DosGame : IGame
    {
        public delegate void OnPlayerSwitched(Player nextPlayer, int unmatchedCardsCount);

        public readonly Dealer Dealer;

        public List<Card> CenterRow = new List<Card>(8);
        public List<List<Card>> CenterRowAdditional = new List<List<Card>>(8);
        public int CurrentPlayerPenalty;

        public readonly Player[] Players;

        public DosGame(Dealer dealer, int players, ushort initialHandSize) : this(
            dealer,
            Enumerable.Range(0, players)
                      .Select(i => new HumanPlayer(i, string.Empty))
                      .Cast<Player>()
                      .ToArray(),
            new GameConfig {InitialHandSize = initialHandSize, StartingPlayer = 0})
        {
        }

        public DosGame(Dealer dealer, Player[] players, GameConfig config)
        {
            Dealer = dealer;
            Config = config;
            Players = players;

            for (var i = 0; i < players.Length; i++)
            {
                Players[i].OrderId = i;
                Players[i].State = PlayerState.WaitingForTurn;
                DealCards(Players[i], config.InitialHandSize, false);
            }

            ResetCurrentState();
            CurrentPlayer = config.StartingPlayer != null && config.StartingPlayer < Players.Length
                ? Players[config.StartingPlayer.Value]
                : Players.RandomElement();
            CurrentPlayer.State = PlayerState.Playing;
            CenterRowSizeAtTurnStart = CenterRow.Count;
        }

        private void ResetCurrentState()
        {
            CurrentState = new TurnStartState(this);
            RefillCenterRow();
        }

        public GameConfig Config { get; }

        public GameState CurrentState { get; set; }

        public bool AllowCallouts { get; set; } = true;

        public Player CurrentPlayer { get; set; }
        public IList<Card> CurrentPlayerHand => CurrentPlayer.Hand;
        public string CurrentPlayerName => CurrentPlayer.Name;

        public int TotalScore => Players.Sum(p => p.HandScore);


        public List<(string name, int cardsCount)> HandsTable => Players.Select(p => (p.Name, p.Hand.Count)).ToList();

        private int CenterRowSizeAtTurnStart { get; set; }
        public int MatchCount { get; set; }

        public int ActivePlayersCount => Players.Count(p => p.IsActive());

        public Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay) =>
            CurrentState.MatchCenterRowCard(player, target, cardsToPlay)
                        .DoIfSuccess(_ => PlayerMatchedCard?.Invoke(player, cardsToPlay, target));

        public Result EndTurn(Player player) => CurrentState.EndTurn(player);

        public Result Draw(Player player) => CurrentState.Draw(player);

        public Result AddCardToCenterRow(Player player, Card card) =>
            CurrentState.AddCardToCenterRow(player, card)
                        .DoIfSuccess(_ => PlayerAddedCard?.Invoke(player, card));

        public Result Callout(Player caller, Player target)
        {
            var calloutTarget = Players.FirstOrDefault(p => p.CanBeCalledOut);
            if (!AllowCallouts || caller == calloutTarget)
                return Result.Fail();

            return CurrentState.Callout(caller, calloutTarget)
                               .DoIfSuccess(_ => CalledOut?.Invoke(caller, calloutTarget))
                               .DoIfFail(_ => FalseCallout?.Invoke(caller));
        }

        public Result CallDos(Player caller) => AllowCallouts
            ? CurrentState.CallDos(caller).DoIfSuccess(_ => DosCall?.Invoke(caller))
            : Result.Fail();

        public event OnPlayerSwitched PlayerSwitched;
        public event Action<Player, Card[]> PlayerReceivedCards;
        public event Action<Player, Card[], Card> PlayerMatchedCard;
        public event Action<Player, Card> PlayerAddedCard;
        public event Action<Player> DosCall;
        public event Action<Player> FalseCallout;
        public event Action<Player, Player> CalledOut;
        public event Action Finished;

        public void DealCard(Player player, bool checkForDos = true)
        {
            DealCards(player, 1, checkForDos);
        }

        public void DealCards(Player player, int amount, bool checkForDos = true)
        {
            if (amount <= 0)
                return;

            var cardsDealt = Enumerable.Range(0, amount)
                                       .Select(_ => DealCardInternal(player, checkForDos))
                                       .TakeWhileNotNull()
                                       .ToArray();

            PlayerReceivedCards?.Invoke(player, cardsDealt);
        }

        private Card? DealCardInternal(Player player, bool checkForDos) =>
            DrawCard().DoIfHasValue(card =>
            {
                player.Hand.Add(card);
                if (checkForDos && player == CurrentPlayer)
                    CheckCurrentPlayerForDos();
            });

        public void CheckCurrentPlayerForDos()
        {
            if (CurrentPlayer.Hand.Count == 2)
                CurrentPlayer.CanBeCalledOut = true;
        }

        private Card? DrawCard() => Dealer.CanDealCards ? (Card?) Dealer.DealCard() : null;

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

            if (CurrentPlayer.Hand.IsEmpty())
            {
                PlayerWentOut(CurrentPlayer);
                if (ActivePlayersCount <= 1)
                {
                    if (ActivePlayersCount == 1)
                        PlayerWentOut(Players.First(p => p.State == PlayerState.WaitingForTurn));
                    SetFinished();
                }
            }
            else
            {
                if (Config.CenterRowPenalty)
                    CurrentPlayerPenalty += unmatchedCardsCount;
                DealCards(CurrentPlayer, CurrentPlayerPenalty);
                CurrentPlayerPenalty = 0;
                CurrentPlayer.State = PlayerState.WaitingForTurn;
            }

            do
            {
                CurrentPlayer = Players[(CurrentPlayer.OrderId + 1) % Players.Length];
            } while (CurrentPlayer.State != PlayerState.WaitingForTurn);

            CurrentPlayer.State = PlayerState.Playing;
            CenterRowSizeAtTurnStart = CenterRow.Count;
            MatchCount = 0;

            PlayerSwitched?.Invoke(CurrentPlayer, unmatchedCardsCount);
        }

        public IEnumerable<string> PersonalGameTableLines(Player player)
        {
            foreach (var p in GameTableLines())
                yield return p;

            foreach (var p in GetPlayerHandLines(player))
                yield return p;
        }

        public static IEnumerable<string> GetPlayerHandLines(Player player)
        {
            var hand = player.Hand;
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

        public void PlayerWentOut(Player player)
        {
            player.State = PlayerState.Out;
            player.ScoreBoardPosition = Players.Where(p => p.ScoreBoardPosition != null)
                                               .Select(p => p.ScoreBoardPosition)
                                               .OrderByDescending(p => p.Value)
                                               .FirstOrDefault() + 1;
        }

        public void SetFinished()
        {
            CurrentState = new FinishedGameState(this);
            Finished?.Invoke();
        }

        public void Quit(Player player)
        {
            switch (ActivePlayersCount)
            {
                case 1:
                    SetFinished();
                    break;
                case 2:
                    PlayerWentOut(Players.First(p => p.IsActive() && p != CurrentPlayer));
                    SetFinished();
                    break;
                default:
                    if (CurrentPlayer == player)
                    {
                        MoveTurnToNextPlayer();
                    }

                    Dealer.DiscardCards(player.Hand);
                    player.CanBeCalledOut = false;
                    player.State = PlayerState.Quit;
                    player.Hand.Clear();

                    ResetCurrentState();
                    break;
            }
        }
    }
}
