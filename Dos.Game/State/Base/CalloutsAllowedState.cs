using Dos.Game.Players;
using Dos.Utils;

namespace Dos.Game.State.Base
{
    public abstract class CalloutsAllowedState : GameState
    {
        protected CalloutsAllowedState(DosGame game) : base(game)
        {
        }

        protected CalloutsAllowedState(GameState gameState) : base(gameState)
        {
        }

        public override Result Callout(Player caller, Player target)
        {
            if (target == null)
            {
                Punish(caller, Config.FalseCalloutPenalty);
                return Config.FalseCalloutPenalty > 0
                    ? Result.Fail(
                        $"False callout! {caller.Name}, draw {Config.FalseCalloutPenalty} more" +
                        (caller == Game.CurrentPlayer ? " after you turn ends." : "."))
                    : Result.Fail("False callout!");
            }

            target.CanBeCalledOut = false;

            if (Config.CalloutPenalty <= 0)
                return Result.Success($"You are right, {target.Name} did not call DOS but there is no penalty");


            Punish(target, Config.CalloutPenalty);

            if (target == CurrentPlayer)
                return Result.Success($"{target.Name}, you have been caught not calling DOS with two cards " +
                                      $"in hand! Draw {Game.CurrentPlayerPenalty} when your turn ends.");

            return Result.Success($"{target.Name}, you have been caught not calling DOS with two cards " +
                                  $"in hand! Draw {Config.CalloutPenalty}.");
        }

        private void Punish(Player player, int amount)
        {
            if (amount <= 0)
                return;
            if (player == CurrentPlayer)
                Game.CurrentPlayerPenalty += amount;
            else
                Game.DealCards(player, amount);
        }

        public override Result CallDos(Player caller)
        {
            if (!caller.CanBeCalledOut)
                return Result.Fail();

            caller.CanBeCalledOut = false;
            return Result.Success($"**DOS! {caller.Name} has only 2 cards!**");
        }
    }
}
