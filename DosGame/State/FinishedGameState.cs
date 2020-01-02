using DosGame.Model;
using DosGame.Util;

namespace DosGame.State
{
    public class FinishedGameState : GameState
    {
        private static readonly Result<string> GameFinishedResult = "Game has finished".ToFail();

        public FinishedGameState(GameState gameState) : base(gameState)
        {
        }

        public override Result<string> MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            GameFinishedResult;

        public override Result<string> FinishMatching(int player) => GameFinishedResult;

        public override Result<string> Draw(int player) => GameFinishedResult;

        public override Result<string> AddCardToCenterRow(int player, Card card) => GameFinishedResult;

        public override Result<string> Callout(int caller) => GameFinishedResult;

        public override Result<string> CallDos(int caller) => GameFinishedResult;
    }
}
