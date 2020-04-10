using System.Collections.Generic;
using System.Linq;
using Dos.Game.Deck;
using Dos.Game.Events;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.State;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game
{
    public class DosGame : IDosGame
    {
        public readonly Player[] Players;

        public readonly List<Card> CenterRow = new List<Card>(8);
        public readonly List<List<Card>> CenterRowAdditional = new List<List<Card>>(8);
        public int CurrentPlayerPenalty;

        public readonly Dealer Dealer;

        public readonly GameEvents Events;

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
            Events = new GameEvents(this);
        }

        public void Start()
        {
            if (CurrentState != null)
                return;

            for (var i = 0; i < Players.Length; i++)
            {
                Players[i].OrderId = i;
                Players[i].State = PlayerState.WaitingForTurn;
                DealCards(Players[i], Config.InitialHandSize, false);
            }

            ResetCurrentState();
            CurrentPlayer = Config.StartingPlayer != null && Config.StartingPlayer < Players.Length
                ? Players[Config.StartingPlayer.Value]
                : Players.RandomElement();
            CurrentPlayer.State = PlayerState.Playing;
            CenterRowSizeAtTurnStart = CenterRow.Count;

            PrivateLog("Game has been started");
            LogCurrentPlayer();
            Events.InvokeGameStarted();
            if (CurrentPlayer.IsAi)
            {
                CurrentPlayer.Play(this);
            }
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

        private int CenterRowSizeAtTurnStart { get; set; }
        public int MatchCount { get; set; }

        public int ActivePlayersCount => Players.Count(p => p.IsActive());

        public Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay) =>
            CurrentState.MatchCenterRowCard(player, target, cardsToPlay)
                        .DoIfSuccess(_ => Events.InvokePlayerMatchedCard(player, target, cardsToPlay));

        public Result EndTurn(Player player) => CurrentState.EndTurn(player);

        public Result Draw(Player player) => CurrentState.Draw(player);

        public Result AddCardToCenterRow(Player player, Card card) =>
            CurrentState.AddCardToCenterRow(player, card)
                        .DoIfSuccess(_ => Events.InvokePlayerAddedCard(player, card));

        public Result Callout(Player caller, Player target)
        {
            var calloutTarget = Players.FirstOrDefault(p => p.CanBeCalledOut);
            if (!AllowCallouts || caller == calloutTarget)
                return Result.Fail();

            return CurrentState.Callout(caller, calloutTarget);
        }

        public Result CallDos(Player caller) => AllowCallouts ? CurrentState.CallDos(caller) : Result.Fail();

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

            PrivateLog($"{player.Name} received [{cardsDealt.ToLogString()}]");
            Events.InvokePlayerReceivedCards(player, cardsDealt);
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

            if (refillNeeded)
            {
                PrivateLog("\n".Join(GameTableLines().Prepend("Center Row refilled:")));
            }

            return refillNeeded;
        }

        public void MoveTurnToNextPlayer(bool currentPlayerQuit = false)
        {
            var unmatchedCardsCount = CenterRowSizeAtTurnStart - MatchCount;

            if (!currentPlayerQuit)
            {
                if (CurrentPlayer.Hand.IsEmpty())
                {
                    PlayerWentOut(CurrentPlayer);
                    if (ActivePlayersCount <= 1)
                    {
                        if (ActivePlayersCount == 1)
                            PlayerWentOut(Players.First(p => p.State == PlayerState.WaitingForTurn), true);
                        SetFinished();
                        return;
                    }
                }
                else
                {
                    if (Config.CenterRowPenalty && unmatchedCardsCount > 0)
                    {
                        CurrentPlayerPenalty += unmatchedCardsCount;
                        PublicLog($"There are {unmatchedCardsCount} unmatched card(s). Draw the same amount.");
                    }

                    DealCards(CurrentPlayer, CurrentPlayerPenalty);
                    CurrentPlayerPenalty = 0;
                    CurrentPlayer.State = PlayerState.WaitingForTurn;
                }
            }

            var prevCurrentPlayer = CurrentPlayer;
            do
            {
                CurrentPlayer = Players[(CurrentPlayer.OrderId + 1) % Players.Length];
            } while (CurrentPlayer.State != PlayerState.WaitingForTurn);


            if (CurrentState.IsFinished)
                return;

            CurrentState = new TurnStartState(this);
            CurrentPlayer.State = PlayerState.Playing;
            CenterRowSizeAtTurnStart = CenterRow.Count;
            MatchCount = 0;

            if (CurrentPlayer != prevCurrentPlayer)
            {
                LogCurrentPlayer();
                Events.InvokePlayerSwitched(prevCurrentPlayer, CurrentPlayer);
            }

            if (CurrentPlayer.IsAi)
            {
                CurrentPlayer.Play(this);
            }
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
            yield return $"Your current hand ({hand.Count.Pluralize("card", "cards")}):";
            yield return "\u200b";
            yield return hand.ToDiscordString();
        }

        public IEnumerable<string> GameTableLines() =>
            CenterRow.Select((t, i) => " - " + t +
                                       (CenterRowAdditional[i]
                                          .Any()
                                           ? $" with {string.Join(" and ", CenterRowAdditional[i])} on top"
                                           : string.Empty));

        public void PlayerWentOut(Player player, bool suppressEvent = false)
        {
            player.State = PlayerState.Out;
            player.ScoreBoardPosition = NextScoreboardPosition;
            if (!suppressEvent)
                Events.InvokeWentOut(player);
        }

        private int NextScoreboardPosition =>
            Players.Select(p => p.ScoreBoardPosition)
                   .WhereHasValue()
                   .MaxOrDefault() + 1;

        public void SetFinished()
        {
            CurrentState = new FinishedGameState(this);
            SetActivePlayersScoreboard();
            Events.InvokeFinished();
        }

        private void SetActivePlayersScoreboard()
        {
            Players.Where(p => p.IsActive())
                   .OrderBy(PlayerScore)
                   .ThenBy(p => (p.OrderId - CurrentPlayer.OrderId + Players.Length) % Players.Length)
                   .ForEach(p => p.ScoreBoardPosition = NextScoreboardPosition);
        }

        private int PlayerScore(Player player) => Config.CardCountRanking
            ? player.Hand.Count
            : player.Hand.Sum(c => c.Points);

        public void Quit(Player player)
        {
            Log($"{player.Name} left the game");
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
                        ClearMatchedCardsFromCenterRow();
                        RefillCenterRow();
                        MoveTurnToNextPlayer(true);
                    }

                    Dealer.DiscardCards(player.Hand);
                    player.CanBeCalledOut = false;
                    player.State = PlayerState.Quit;
                    player.Hand.Clear();

                    break;
            }
        }

        public void ClearMatchedCardsFromCenterRow()
        {
            var clearedAnything = false;
            for (var i = 0; i < CenterRow.Count; i++)
            {
                if (CenterRowAdditional[i].IsEmpty())
                    continue;
                clearedAnything = true;
                Dealer.DiscardCard(CenterRow[i]);
                CenterRow.RemoveAt(i);
                Dealer.DiscardCards(CenterRowAdditional[i]);
                CenterRowAdditional.RemoveAt(i);
                i--;
            }

            if (clearedAnything)
            {
                PrivateLog("Cleared Center Row");
            }
        }

        public void PublicLog(string message) => Events.InvokePublicLog(message);
        public void PrivateLog(string message) => Events.InvokePrivateLog(message);

        public void Log(string message)
        {
            PrivateLog(message);
            PublicLog(message);
        }

        private void LogCurrentPlayer()
        {
            PrivateLog($"Now it's {CurrentPlayer.Name}'s turn, hand: {CurrentPlayer.Hand.ToLogString()}");
        }

        public Result SwapWith(Player caller, Player target) =>
            target.IsActive()
                ? CurrentState.SwapWith(caller, target)
                : Result.Fail("You can't swap with unactive player");
    }
}
