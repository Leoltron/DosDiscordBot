using System;
using Dos.Game.Match;

namespace Dos.Game.Extensions
{
    public static class MatchTypeExtensions
    {
        public static MatchResult ToResult(this MatchType type, GameConfig config = null) => new(type, type.Message(config));

        public static string Message(this MatchType type, GameConfig config = null) =>
            type switch
            {
                MatchType.NoMatch => "No matching card found",
                MatchType.SingleMatch => "Single Number match",
                MatchType.DoubleMatch => "Double Number match!",
                MatchType.SingleColorMatch => "Single Color Match! " +
                                              "After matching, place one more card to the Center Row",
                MatchType.DoubleColorMatch => "Double Color Match!! " +
                                              "After matching, place one more card to the Center Row. " +
                                              "Also, everyone else, draw " + (config?.DoubleColorMatchDraw ?? 1),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static (int discardCount, int drawCount)
            ToColorMatchBonus(this MatchType type, GameConfig config = null) =>
            type switch
            {
                MatchType.NoMatch => (0, 0),
                MatchType.SingleMatch => (0, 0),
                MatchType.DoubleMatch => (0, 0),
                MatchType.SingleColorMatch => (1, 0),
                MatchType.DoubleColorMatch => (1, config?.DoubleColorMatchDraw ?? 1),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static bool IsColorMatch(this MatchType type) =>
            type switch
            {
                MatchType.SingleColorMatch => true,
                MatchType.DoubleColorMatch => true,
                _ => false
            };
    }
}
