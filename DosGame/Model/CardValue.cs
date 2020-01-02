namespace DosGame.Model
{
    public enum CardValue : byte
    {
        Sharp = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10
    }

    public static class CardValueExtensions
    {
        private static int MinValue(this CardValue value)
        {
            return value == CardValue.Sharp ? 1 : (int) value;
        }

        public static bool Matches(this CardValue target, CardValue other)
        {
            return target == CardValue.Sharp || other == CardValue.Sharp || target == other;
        }

        public static bool Matches(this CardValue target, CardValue first, CardValue second)
        {
            if (target == CardValue.Sharp)
            {
                return first.MinValue() + second.MinValue() <= 10;
            }

            var targetValue = (int) target;

            if (first == CardValue.Sharp)
            {
                return second.MinValue() < targetValue;
            }

            if (second == CardValue.Sharp)
            {
                return first.MinValue() < targetValue;
            }

            return (int) first + (int) second < targetValue;
        }

        public static string Name(this CardValue value)
        {
            return value == CardValue.Sharp ? "#" : ((byte) value).ToString();
        }
    }
}
