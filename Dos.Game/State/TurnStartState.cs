using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class TurnStartState : BaseCurrentPlayerState
    {
        public TurnStartState(Game game) : base(game)
        {
        }

        public TurnStartState(GameState gameState) : base(gameState)
        {
        }

        protected override Result CurrentPlayerDraw()
        {
            Game.DealCard(CurrentPlayer);
            Game.CurrentState = new JustDrewCardState(this);

            return Result.Success("Here's your card. Now make a match or place one card to the center row.");
        }

        protected override Result CurrentPlayerEndTurn() => Result.Fail("You need to make a match or draw.");

        protected override Result CurrentPlayerAddCardToCenterRow(Card card) => Result.Fail("You can't add cards now");
    }
}
