using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class FinishedGameState : GameState
    {
        private static readonly Result GameFinishedResult = Result.Fail("Game has finished");

        public FinishedGameState(GameState gameState) : base(gameState)
        {
        }

        public override bool IsFinished => true;

        public override Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            GameFinishedResult;

        public override Result EndTurn(int player) => GameFinishedResult;

        public override Result Draw(int player) => GameFinishedResult;

        public override Result AddCardToCenterRow(int player, Card card) => GameFinishedResult;

        public override Result Callout(int caller) => GameFinishedResult;

        public override Result CallDos(int caller) => GameFinishedResult;
    }
}
