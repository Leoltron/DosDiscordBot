using Dos.Game.Model;
using Dos.Game.Players;
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

        public sealed override Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay) =>
            player.State == PlayerState.Playing
                ? CurrentPlayerMatchCenterRowCard(target, cardsToPlay)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay);

        public sealed override Result Draw(Player player) =>
            player.State == PlayerState.Playing
                ? CurrentPlayerDraw()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerDraw();

        public sealed override Result EndTurn(Player player) =>
            player.State == PlayerState.Playing
                ? CurrentPlayerEndTurn()
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerEndTurn();

        public sealed override Result AddCardToCenterRow(Player player, Card card) =>
            player.State == PlayerState.Playing
                ? CurrentPlayerAddCardToCenterRow(card)
                : NonCurrentPlayerFail;

        protected abstract Result CurrentPlayerAddCardToCenterRow(Card card);
    }
}
