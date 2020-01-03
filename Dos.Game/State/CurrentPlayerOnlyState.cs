using Dos.Game.Model;
using Dos.Game.Util;

namespace Dos.Game.State
{
    public abstract class CurrentPlayerOnlyState : CalloutsAllowedState
    {
        private static readonly Result<string> NonCurrentPlayerFail = "It's not your turn right now.".ToFail();

        protected CurrentPlayerOnlyState(Game game) : base(game)
        {
        }

        protected CurrentPlayerOnlyState(GameState gameState) : base(gameState)
        {
        }

        public override Result<string> MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerMatchCenterRowCard(target, cardsToPlay)
                : NonCurrentPlayerFail;

        protected abstract Result<string> CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay);

        public override Result<string> Draw(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerDraw()
                : NonCurrentPlayerFail;

        protected abstract Result<string> CurrentPlayerDraw();

        public override Result<string> FinishMatching(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerFinishMatching()
                : NonCurrentPlayerFail;

        protected abstract Result<string> CurrentPlayerFinishMatching();

        public override Result<string> AddCardToCenterRow(int player, Card card) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerAddCardToCenterRow(card)
                : NonCurrentPlayerFail;

        protected abstract Result<string> CurrentPlayerAddCardToCenterRow(Card card);
    }
}
