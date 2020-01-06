using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State
{
    public abstract class CurrentPlayerOnlyState : CalloutsAllowedState
    {
        private static readonly Result NonCurrentPlayerFail = Result.Fail("It's not your turn right now.");

        protected CurrentPlayerOnlyState(Game game) : base(game)
        {
        }

        protected CurrentPlayerOnlyState(GameState gameState) : base(gameState)
        {
        }

        public override Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerMatchCenterRowCard(target, cardsToPlay)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay);

        public override Result Draw(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerDraw()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerDraw();

        public override Result FinishMatching(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerFinishMatching()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerFinishMatching();

        public override Result AddCardToCenterRow(int player, Card card) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerAddCardToCenterRow(card)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerAddCardToCenterRow(Card card);
    }
}
