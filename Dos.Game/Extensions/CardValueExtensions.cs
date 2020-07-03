using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class CardValueExtensions
    {
        private static int MinValue(this CardValue value) => value == CardValue.Sharp ? 1 : (int) value;

        public static bool Matches(this CardValue target, CardValue other) =>
            target == CardValue.Sharp || other == CardValue.Sharp || target == other;

        public static bool Matches(this CardValue target, CardValue first, CardValue second)
        {
            if (target == CardValue.Sharp)
                return first.MinValue() + second.MinValue() <= 10;

            var targetValue = (int) target;

            if (first == CardValue.Sharp)
                return second.MinValue() < targetValue;

            if (second == CardValue.Sharp)
                return first.MinValue() < targetValue;

            return (int) first + (int) second == targetValue;
        }

        public static string Name(this CardValue value) => value == CardValue.Sharp ? "#" : ((byte) value).ToString();

        public static string UrlName(this CardValue value) =>
            value == CardValue.Sharp ? "S" : ((byte) value).ToString();
    }
}
