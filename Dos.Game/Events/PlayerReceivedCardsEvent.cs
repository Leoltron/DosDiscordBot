using Dos.Game.Model;
using Dos.Game.Players;

namespace Dos.Game.Events
{
    public class PlayerReceivedCardsEvent : PlayerEvent
    {
        public Card[] Cards { get; }

        public PlayerReceivedCardsEvent(DosGame game, Player player, Card[] cards) : base(game, player)
        {
            Cards = cards;
        }
    }
}
