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
                Game.PrivateLog($"{caller} made a false callout");
                Punish(caller, Config.FalseCalloutPenalty);

                if (Config.FalseCalloutPenalty > 0)
                    Game.PublicLog($"False callout! {caller.Name}, draw {Config.FalseCalloutPenalty} more" +
                                   (caller == Game.CurrentPlayer ? " after you turn ends." : "."));
                else
                    Game.PublicLog("False callout!");

                Game.Events.InvokeFalseCallout(caller);
                    
                return Result.Fail();
            }

            target.CanBeCalledOut = false;

            if (Config.CalloutPenalty <= 0)
                return Result.Success($"You are right, {target.Name} did not call DOS but there is no penalty");


            Game.PrivateLog($"{caller} called out {target}");
            Punish(target, Config.CalloutPenalty);
            Game.Events.InvokeCalledOut(caller, target);

            Game.PublicLog(target == CurrentPlayer
                               ? $"{target.Name}, you have been caught not calling DOS with two cards " +
                                 $"in hand! Draw {Game.CurrentPlayerPenalty} when your turn ends."
                               : $"{target.Name}, you have been caught not calling DOS with two cards " +
                                 $"in hand! Draw {Config.CalloutPenalty}.");

            return Result.Success();
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
            Game.Events.InvokeDosCall(caller);
            Game.PrivateLog($"{caller} called DOS");
            return Result.Success($"**DOS! {caller.Name} has only 2 cards!**");
        }
    }
}
