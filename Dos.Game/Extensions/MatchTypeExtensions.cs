using System;
using Dos.Game.Match;

namespace Dos.Game.Extensions
{
    public static class MatchTypeExtensions
    {
        public static MatchResult DefaultResult(this MatchType type)
        {
            return type switch
            {
                MatchType.NoMatch => MatchResult.NoMatch("No matching card found"),
                MatchType.SingleMatch => MatchResult.NoMatch("Single Number match"),
                MatchType.DoubleMatch => MatchResult.NoMatch("Double Number match!"),
                MatchType.SingleColorMatch => MatchResult.NoMatch("Single Color Match! " +
                                                                  "After matching, place one more card to the Center Row"),
                MatchType.DoubleColorMatch => MatchResult.NoMatch("Double Color Match!! " +
                                                                  "After matching, place one more card to the Center Row. Also, everyone else, draw 1"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static (int discardCount, int drawCount) ToColorMatchBonus(this MatchType type)
        {
            return type switch
            {
                MatchType.NoMatch => (0, 0),
                MatchType.SingleMatch => (0, 0),
                MatchType.DoubleMatch => (0, 0),
                MatchType.SingleColorMatch => (1, 0),
                MatchType.DoubleColorMatch => (1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
