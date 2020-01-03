using System;

namespace Dos.Game.Match
{
    public enum MatchType
    {
        NoMatch,
        SingleMatch,
        DoubleMatch,
        SingleColorMatch,
        DoubleColorMatch
    }

    public static class MatchTypeExtensions
    {
        public static MatchResult DefaultResult(this MatchType type)
        {
            switch (type)
            {
                case MatchType.NoMatch:
                    return MatchResult.NoMatch("No matching card found");
                case MatchType.SingleMatch:
                    return MatchResult.NoMatch("Single match!");
                case MatchType.DoubleMatch:
                    return MatchResult.NoMatch("Double match!");
                case MatchType.SingleColorMatch:
                    return MatchResult.NoMatch("Single Color Match! Discard one card");
                case MatchType.DoubleColorMatch:
                    return MatchResult.NoMatch("Double Color Match! Discard one card, everyone has to pick a card");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static (int discardCount, int drawCount) ToColorMatchBonus(this MatchType type)
        {
            switch (type)
            {
                case MatchType.NoMatch:
                case MatchType.SingleMatch:
                case MatchType.DoubleMatch:
                    return (0, 0);
                case MatchType.SingleColorMatch:
                    return (1, 0);
                case MatchType.DoubleColorMatch:
                    return (1, 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
