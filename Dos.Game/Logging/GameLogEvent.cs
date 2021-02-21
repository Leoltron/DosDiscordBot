using System.Linq;
using Dos.Game.Model;

namespace Dos.Game.Logging
{
    public class GameLogEvent
    {
        public GameLogEventType EventType { get; }
        public int? SourcePlayer { get; }
        public int? TargetPlayer { get; }
        public Card[] Cards { get; }

        public GameLogEvent(GameLogEventType eventType, int? sourcePlayer = null, int? targetPlayer = null, params Card[] cards)
        {
            EventType = eventType;
            SourcePlayer = sourcePlayer;
            TargetPlayer = targetPlayer;
            Cards = cards;
        }

        public static GameLogEvent PlayerReceivedCards(int player, params Card[] cards) =>
            new(GameLogEventType.PlayerReceivedCard, targetPlayer: player, cards:cards);

        public static GameLogEvent CenterRowMatch(int player, Card target, params Card[] matchers) =>
            new(GameLogEventType.CenterRowMatch, player, cards: matchers.Prepend(target).ToArray());

        public static GameLogEvent CenterRowAdd(params  Card[] cards) =>
            new(GameLogEventType.CenterRowAdd, cards: cards);

        public static GameLogEvent CenterRowPlayerAdd(int player, Card card) =>
            new(GameLogEventType.CenterRowPlayerAdd, player, cards: card);

        public static GameLogEvent ClearCenterRow() => new(GameLogEventType.ClearCenterRow);

        public static GameLogEvent PlayerTurnStart(int player) =>
            new(GameLogEventType.PlayerTurnStart, player);

        public static GameLogEvent PlayerGoOut(int player) => new(GameLogEventType.PlayerGoOut, player);

        public static GameLogEvent PlayersSwappedHands(int player, int targetPlayer) =>
            new(GameLogEventType.PlayersSwappedHands, player, targetPlayer);
    }
}
