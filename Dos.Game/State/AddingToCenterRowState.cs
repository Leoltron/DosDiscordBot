using Dos.Game.Model;
using Dos.Game.State.Base;
using Dos.Utils;

namespace Dos.Game.State
{
    public class AddingToCenterRowState : BaseCurrentPlayerState
    {
        private static readonly Result MatchingFail = Result.Fail("You have already finished matching cards");

        public AddingToCenterRowState(GameState gameState, int cardsToAdd) : base(gameState)
        {
            CardsToAdd = cardsToAdd;
        }

        public override bool CanMatch => false;
        public override bool CanAdd => true;
        public override bool CanDraw => false;

        protected override Result CurrentPlayerMatchCenterRowCard(Card target, Card[] cardsToPlay) =>
            MatchingFail;
    }
}
