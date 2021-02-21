using System;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Players
{
    public class WeightAiPlayer : AiPlayer
    {
        private readonly WeightConfig weightConfig;

        public WeightAiPlayer(int orderId, WeightConfig weightConfig) : base(orderId)
        {
            this.weightConfig = weightConfig;
        }
        
        public WeightAiPlayer(int orderId, WeightConfig weightConfig, string name) : base(orderId, name)
        {
            this.weightConfig = weightConfig;
        }

        protected override (Card[] matchers, Card target)? MakeMatch(DosGame game) =>
            AiHelpers.GetAvailableMatches(
                          Hand, game.CenterRow.Where((_, i) => game.CenterRowAdditional[i].IsEmpty()).ToList()).ToList()
                     .Select(m => (m, Math.Abs(ScoreMatch(m))))
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

            var coef = GetCoefficient(m);
            return coef < 0
                ? -coef * matchers.Select(c => 10 - (int) c.Value).Sum(v => v * v)
                : coef * matchers.Select(c => (int) c.Value).Sum(v => v * v);
        }

        private int GetCoefficient(MatchType matchType) =>
            matchType switch
            {
                MatchType.DoubleColorMatch => weightConfig.DoubleColorMatchCoef,
                MatchType.SingleColorMatch => weightConfig.SingleColorMatchCoef,
                MatchType.SingleMatch => weightConfig.SingleMatchCoef,
                MatchType.DoubleMatch => weightConfig.DoubleMatchCoef,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
