using System;

namespace DosGame
{
    public struct Card : IEquatable<Card>
    {
        public static readonly Card WildDos = CardValue.Two.Of(CardColor.Wild);
        public static readonly Card WildSharp = CardValue.Sharp.Of(CardColor.Wild);
        
        public CardColor Color { get; }
        public CardValue Value { get; }

        public Card(CardColor color, CardValue value)
        {
            Color = color;
            Value = value;
        }

        public int Points => Value == CardValue.Sharp
            ? 40
            : Color == CardColor.Wild
                ? 20
                : (int) Value;

        public bool Equals(Card other)
        {
            return Color == other.Color && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Card other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) Color, (int) Value);
        }

        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => this == WildDos ? "Wild Dos" : $"{Color} {Value.Name()}";
    }
}
