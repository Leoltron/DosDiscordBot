using System;
using System.Collections.Generic;
using System.Linq;
using Dos.Game.Events;
using Dos.Game.Extensions;
using Dos.Utils;

namespace Dos.Game.Logging
{
    public class GameLogger
    {
        private string[] playerNames;
        protected readonly DosGame game;
        private readonly List<GameLogEvent> events = new();
        private readonly List<GameSnapshot> snapshots = new();
        private readonly List<string> eventStrings = new();

        public GameLogger(DosGame game)
        {
            this.game = game;
            var gameEvents = game.Events;

            if (game.HasStarted)
                CopyPlayerNamesFromGame();
            else
                gameEvents.GameStarted += _ => CopyPlayerNamesFromGame();
            gameEvents.CenterRowRefilled += OnAddedCard;
            gameEvents.PlayerSwitched += OnPlayerSwitched;
            gameEvents.PlayerReceivedCards += OnPlayerReceivedCards;
            gameEvents.PlayerMatchedCard += OnPlayerMatchedCard;
            gameEvents.PlayerAddedCard += OnPlayerAddedCard;
            gameEvents.ClearCenterRow += () => AppendEvent(GameLogEvent.ClearCenterRow());
            gameEvents.WentOut += e => AppendEvent(GameLogEvent.PlayerGoOut(e.Player.OrderId));
            gameEvents.PlayersSwappedHands +=
                e => AppendEvent(GameLogEvent.PlayersSwappedHands(e.Player.OrderId, e.Target.OrderId));
        }

        private void CopyPlayerNamesFromGame()
        {
            playerNames = game.Players.Select(p => p.Name).ToArray();
        }

        private void FillUpEvents()
        {
            snapshots.Add(game.ToSnapshot());

            var s =
                $"[{string.Join(", ", game.CenterRow.Zip(game.CenterRowAdditional, (c, cs) => cs.Prepend(c)).Select(cs => "+".Join(cs.Select(c => c.ShortName))))}] 0: ({", ".Join(game.Players[0].Hand.OrderByColorAndValue().Select(c => c.ShortName))}) 1: ({", ".Join(game.Players[1].Hand.OrderByColorAndValue().Select(c => c.ShortName))})";
            eventStrings.Add(s);
        }

        private void OnAddedCard(CenterRowRefilledEvent e)
        {
            AppendEvent(GameLogEvent.CenterRowAdd(e.Cards));
        }

        private void OnPlayerAddedCard(CenterRowPlayerAddedCardEvent e)
        {
            AppendEvent(GameLogEvent.CenterRowPlayerAdd(e.Player.OrderId, e.Card));
        }

        private void OnPlayerMatchedCard(CardMatchedEvent e)
        {
            AppendEvent(GameLogEvent.CenterRowMatch(e.Player.OrderId, e.Target, e.MatchingCards));
        }

        private void OnPlayerReceivedCards(PlayerReceivedCardsEvent e)
        {
            AppendEvent(GameLogEvent.PlayerReceivedCards(e.Player.OrderId, e.Cards));
        }

        private void OnPlayerSwitched(PlayerSwitchedEvent e)
        {
            AppendEvent(GameLogEvent.PlayerTurnStart(e.NextPlayer.OrderId));
        }

        private void AppendEvent(GameLogEvent gameLogEvent)
        {
            AppendEvent(gameLogEvent, game.ToSnapshot());
        }

        protected virtual void AppendEvent(GameLogEvent gameLogEvent, GameSnapshot gameSnapshot)
        {
            events.Add(gameLogEvent);
            snapshots.Add(gameSnapshot);
        }

        public List<GameLogEvent> Events => events.ToList();
        public List<string> EventStrings => eventStrings.ToList();

        public GameReplay BuildReplay() => game.HasStarted
            ? new GameReplay(playerNames, events.ToArray(), snapshots.ToArray(), DateTime.UtcNow)
            : null;
    }
}
