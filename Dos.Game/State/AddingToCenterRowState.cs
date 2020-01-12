using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class AddingToCenterRowState : BaseCurrentPlayerState
    {
        private static readonly Result MatchingFail = Result.Fail("You are already finished matching cards");

        public AddingToCenterRowState(GameState gameState, int cardsToAdd) : base(gameState) =>
            CardsToAdd = cardsToAdd;

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay) =>
            MatchingFail;
    }
}
