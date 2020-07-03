using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class TurnStartState : BaseCurrentPlayerState
    {
        public TurnStartState(DosGame game) : base(game)
        {
        }

        public TurnStartState(GameState gameState) : base(gameState)
        {
        }

        public override bool CanMatch => true;
        public override bool CanAdd => false;
        public override bool CanDraw => true;

        protected override Result CurrentPlayerDraw()
        {
            Game.Events.InvokePlayerIsGoingToDraw(CurrentPlayer);
            if (Game.Config.DrawEndsTurn)
            {
                Game.CurrentState = new AddingToCenterRowState(this, 0);
                if (!Game.Config.CenterRowPenalty)
                {
                    Game.CurrentPlayerPenalty += 1;
                    Game.PublicLog("Here's your card. Also, skip a turn.");
                }
                else
                {
                    Game.PublicLog("Skip a turn.");
                }

                Game.CurrentState.EndTurn(CurrentPlayer);

                return Result.Success();
            }

            Game.DealCard(CurrentPlayer);
            Game.CurrentState = new JustDrewCardState(this);

            return Result.Success("Here's your card. Now make a match or place one card to the Center Row.");
        }

        protected override Result CurrentPlayerEndTurn() => Result.Fail("You need to make a match or draw.");

        protected override Result CurrentPlayerAddCardToCenterRow(Card card) => Result.Fail("You can't add cards now");
    }
}
