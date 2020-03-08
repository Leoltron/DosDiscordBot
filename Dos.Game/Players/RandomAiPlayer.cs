using System.Linq;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Players
{
    public class RandomAiPlayer : AiPlayer
    {
        public RandomAiPlayer(int orderId, string name) : base(orderId, name)
        {
        }

        public RandomAiPlayer(int orderId) : base(orderId)
        {
        }

        protected override (Card[] matchers, Card target)? MakeMatch(DosGame game) =>
            AiHelpers.GetAvailableMatches(
                          Hand, game.CenterRow.Where((c, i) => game.CenterRowAdditional[i].IsEmpty()).ToList()).ToList()
                     .RandomElementOrDefault();

        protected override Card ChooseCardForAdding(DosGame game) => Hand.RandomElement();
    }
}
