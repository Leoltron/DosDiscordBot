using Dos.Utils;

namespace Dos.Game.State.Base
{
    public abstract class CalloutsAllowedState : GameState
    {
        protected CalloutsAllowedState(Game game) : base(game)
        {
        }

        protected CalloutsAllowedState(GameState gameState) : base(gameState)
        {
        }

        public override Result Callout(int caller)
        {
            if (Game.PlayerWhoDidNotCallDos == null)
                return Game.FalseCalloutPenalty > 0
                    ? Result.Success($"False callout! {Game.GetPlayerName(caller)}, draw {Game.FalseCalloutPenalty}.")
                    : Result.Fail("False callout!");

            var victimIndex = Game.PlayerWhoDidNotCallDos.Value;
            var victimName = Game.GetPlayerName(victimIndex);

            Game.PlayerWhoDidNotCallDos = null;

            if (Game.CalloutPenalty <= 0)
                return Result.Success($"You are right, {victimName} did not call DOS but there is no penalty");

            if (victimIndex == CurrentPlayer)
            {
                Game.CurrentPlayerPenalty += Game.CalloutPenalty;
                return Result.Success($"{victimName}, you have been caught not calling DOS with two cards " +
                                      $"in hand! Draw {Game.CurrentPlayerPenalty} when your turn ends.");
            }

            Game.DealCards(victimIndex, Game.CalloutPenalty);
            return Result.Success($"{victimName}, you have been caught not calling DOS with two cards " +
                                  $"in hand! Draw {Game.CurrentPlayerPenalty}.");
        }

        public override Result CallDos(int caller)
        {
            if (Game.PlayerWhoDidNotCallDos != caller)
                return Result.Fail();

            Game.PlayerWhoDidNotCallDos = null;
            return Result.Success($"**DOS! {Game.GetPlayerName(caller)} has only 2 cards!**");
        }
    }
}
