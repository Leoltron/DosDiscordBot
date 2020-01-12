using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class JustDrewCardState : BaseCurrentPlayerState
    {
        public JustDrewCardState(GameState gameState) : base(gameState)
        {
            DrewCard = true;
        }

        protected override Result CurrentPlayerAddCardToCenterRow(Card card)
        {
            CardsToAdd = 1;
            return base.CurrentPlayerAddCardToCenterRow(card);
        }

        protected override Result CurrentPlayerEndTurn() =>
            Result.Fail("You must add card to the Center Row or make a match first.");
    }
}
