using System;
using Dos.Game.Extensions;

namespace Dos.Game.Model
{
    public struct Card : IEquatable<Card>
    {
        public static readonly Card WildDos = 2.Of(CardColor.Wild);

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

        public bool Equals(Card other) => Color == other.Color && Value == other.Value;

        public override bool Equals(object obj) => obj is Card other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int) Color, (int) Value);

        public static bool operator ==(Card left, Card right) => left.Equals(right);

        public static bool operator !=(Card left, Card right) => !left.Equals(right);

        public override string ToString() => this == WildDos ? "Wild Dos" : $"{Color} {Value.Name()}";

        public string ToShortString() => Color.ShortName() + Value.Name();
    }
}
