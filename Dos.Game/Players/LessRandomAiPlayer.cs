using System;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Players
{
    public class LessRandomAiPlayer : AiPlayer
    {
        public LessRandomAiPlayer(int orderId, string name) : base(orderId, name)
        {
        }

        public LessRandomAiPlayer(int orderId) : base(orderId)
        {
        }

        protected override (Card[] matchers, Card target)? MakeMatch(DosGame game) =>
            AiHelpers.GetAvailableMatches(
                          Hand, game.CenterRow.Where((_, i) => game.CenterRowAdditional[i].IsEmpty()).ToList()).ToList()
                     .Select(m => (m, ScoreMatch(m)))
                     .OrderByDescending(pair => pair.Item2)
                     .Cast<((Card[], Card), int)?>()
                     .FirstOrDefault()?.Item1;

        protected override Card ChooseCardForAdding(DosGame game) =>
            Hand.Where(c => c.Value != CardValue.Sharp).OrderByDescending(c => c.Value).Cast<Card?>()
                .FirstOrDefault() ?? Hand.RandomElement();

        private int ScoreMatch((Card[] matchers, Card target) match)
        {
            var (matchers, target) = match;
            var m = target.MatchWith(matchers);

            var sum = matchers.Sum(c => (int)c.Value);

            switch (m)
            {
                case MatchType.DoubleColorMatch:
                    return 2000*sum;
                case MatchType.SingleColorMatch:
                    return 1000 * sum;
                case MatchType.SingleMatch:
                case MatchType.DoubleMatch:
                    return sum;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
