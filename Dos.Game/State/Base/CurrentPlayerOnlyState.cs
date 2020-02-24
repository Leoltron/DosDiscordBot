using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State.Base
{
    public abstract class CurrentPlayerOnlyState : CalloutsAllowedState
    {
        private static readonly Result NonCurrentPlayerFail = Result.Fail("It's not your turn right now.");

        protected CurrentPlayerOnlyState(DosGame game) : base(game)
        {
        }

        protected CurrentPlayerOnlyState(GameState gameState) : base(gameState)
        {
        }

        public sealed override Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerMatchCenterRowCard(target, cardsToPlay)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay);

        public sealed override Result Draw(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerDraw()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerDraw();

        public sealed override Result EndTurn(int player) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerEndTurn()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerEndTurn();

        public sealed override Result AddCardToCenterRow(int player, Card card) =>
            player == Game.CurrentPlayer
                ? CurrentPlayerAddCardToCenterRow(card)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerAddCardToCenterRow(Card card);
    }
}
