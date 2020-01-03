using Dos.Game.Util;

namespace Dos.Game.State
{
    public abstract class CalloutsAllowedState : GameState
    {
        protected CalloutsAllowedState(Game game) : base(game)
        {
        }

        protected CalloutsAllowedState(GameState gameState) : base(gameState)
        {
        }

        public override Result<string> Callout(int caller)
        {
            if (!Game.CurrentPlayerDidNotCallDos)
                return Game.FalseCalloutPenalty > 0
                    ? $"False callout! {Game.GetPlayerName(caller)}, draw {Game.FalseCalloutPenalty}.".ToSuccess()
                    : "False callout!".ToFail();


            Game.CurrentPlayerDidNotCallDos = false;

            if (Game.CalloutPenalty <= 0)
                return $"You are right, {CurrentPlayerName} did not call DOS but there is no penalty".ToSuccess();

            Game.CurrentPlayerPenalty += Game.CalloutPenalty;
            return ($"{CurrentPlayerName}, you have been caught not calling DOS with two cards " +
                    $"in hand! Draw {Game.CurrentPlayerPenalty} when your turn ends.").ToSuccess();
        }

        public override Result<string> CallDos(int caller)
        {
            if (caller != CurrentPlayer)
                return "You do not need to call DOS if you got 2 cards outside of your turn or you already finished it."
                   .ToFail();

            var cardsCount = Game.playerHands[caller].Count;
            if (cardsCount != 2 && !Game.CurrentPlayerDidNotCallDos)
                return $"You do not have to call DOS since you have {cardsCount} cards, not 2".ToFail();

            Game.CurrentPlayerDidNotCallDos = false;
            return $"**DOS! {Game.GetPlayerName(caller)} has only 2 cards!**".ToSuccess();
        }
    }
}
