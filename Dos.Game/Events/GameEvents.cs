using System;
using Dos.Game.Model;
using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class GameEvents
    {
        private readonly DosGame game;

        public GameEvents(DosGame game)
        {
            this.game = game;
        }

        public event Action<Event> GameStarted;
        public void InvokeGameStarted() => GameStarted?.Invoke(new Event(game));

        public event Action<PlayerReceivedCardsEvent> PlayerReceivedCards;

        public void InvokePlayerReceivedCards(Player player, Card[] cards) =>
            PlayerReceivedCards?.Invoke(new PlayerReceivedCardsEvent(game, player, cards));

        public event Action<LogEvent> PublicLog;
        public void InvokePublicLog(string message) => PublicLog?.Invoke(new LogEvent(game, message));

        public event Action<LogEvent> PrivateLog;
        public void InvokePrivateLog(string message) => PrivateLog?.Invoke(new LogEvent(game, message));

        public event Action<PlayerSwitchedEvent> PlayerSwitched;

        public void InvokePlayerSwitched(Player previousPlayer, Player nextPlayer) =>
            PlayerSwitched?.Invoke(new PlayerSwitchedEvent(game, previousPlayer, nextPlayer));

        public event Action<CardMatchedEvent> PlayerMatchedCard;

        public void InvokePlayerMatchedCard(Player player, Card target, Card[] matchingCards) =>
            PlayerMatchedCard?.Invoke(new CardMatchedEvent(game, player, target, matchingCards));

        public event Action<CenterRowAddedCardEvent> PlayerAddedCard;

        public void InvokePlayerAddedCard(Player player, Card card) =>
            PlayerAddedCard?.Invoke(new CenterRowAddedCardEvent(game, player, card));

        public event Action<PlayerEvent> PlayerIsGoingToDraw;

        public void InvokePlayerIsGoingToDraw(Player player) =>
            PlayerIsGoingToDraw?.Invoke(new PlayerEvent(game, player));

        public event Action<PlayerEvent> DosCall;
        public void InvokeDosCall(Player player) => DosCall?.Invoke(new PlayerEvent(game, player));

        public event Action<PlayerEvent> FalseCallout;
        public void InvokeFalseCallout(Player player) => FalseCallout?.Invoke(new PlayerEvent(game, player));

        public event Action<CalledOutEvent> CalledOut;
        public void InvokeCalledOut(Player caller, Player target) =>
            CalledOut?.Invoke(new CalledOutEvent(game, caller, target));

        public event Action<PlayersSwappedHandsEvent> PlayersSwappedHands;
        public void InvokePlayersSwappedHands(Player caller, Player target) =>
            PlayersSwappedHands?.Invoke(new PlayersSwappedHandsEvent(game, caller, target));

        public event Action<PlayerEvent> WentOut;
        public void InvokeWentOut(Player player) => WentOut?.Invoke(new PlayerEvent(game, player));

        public event Action Finished;
        public void InvokeFinished() => Finished?.Invoke();
    }
}
