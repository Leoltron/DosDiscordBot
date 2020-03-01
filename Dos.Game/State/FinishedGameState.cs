using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class FinishedGameState : GameState
    {
        private static readonly Result GameFinishedResult = Result.Fail("Game has finished");

        public FinishedGameState(DosGame game) : base(game)
        {
        }

        public override bool IsFinished => true;

        public override Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay) =>
            GameFinishedResult;

        public override Result EndTurn(Player player) => GameFinishedResult;

        public override Result Draw(Player player) => GameFinishedResult;

        public override Result AddCardToCenterRow(Player player, Card card) => GameFinishedResult;

        public override Result Callout(Player caller, Player target) => GameFinishedResult;

        public override Result SwapWith(Player caller, Player target) => GameFinishedResult;

        public override Result CallDos(Player caller) => GameFinishedResult;
    }
}
