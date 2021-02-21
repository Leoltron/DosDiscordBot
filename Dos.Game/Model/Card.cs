using System;
using System.Collections.Generic;
using Dos.Game.Extensions;
using Newtonsoft.Json;

namespace Dos.Game.Model
{
    public readonly struct Card : IEquatable<Card>
    {
        public static readonly Card WildDos = 2.Of(CardColor.Wild);

        public CardColor Color { get; }
        public CardValue Value { get; }

        public Card(CardColor color, CardValue value)
        {
            Color = color;
            Value = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public int Points => Value == CardValue.Sharp
            ? 40
            : Color == CardColor.Wild
                ? 20
                : (int) Value;

        public bool Equals(Card other) => Color == other.Color && Value == other.Value;

        public override bool Equals(object obj) => obj is Card other && Equals(other);

        public override int GetHashCode() => (byte)Color << 8 | (byte)Value;

        public static bool operator ==(Card left, Card right) => left.Equals(right);

        public static bool operator !=(Card left, Card right) => !left.Equals(right);

        public override string ToString() => GetOrGenerateCardString(this);

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string ShortName => GetOrGenerateCardShortString(this);

        private static readonly Dictionary<Card, string> CardStrings = new();

        private static string GetOrGenerateCardString(Card card)
        {
            if (CardStrings.TryGetValue(card, out var s))
                return s;
            
            return CardStrings[card] = card == WildDos ? "Wild Dos" : $"{card.Color} {card.Value.Name()}";
        }

        private static readonly Dictionary<Card, string> CardShortStrings = new();

        private static string GetOrGenerateCardShortString(Card card)
        {
            if (CardShortStrings.TryGetValue(card, out var s))
                return s;
            
            return CardShortStrings[card] = card.Color.ShortName() + card.Value.UrlName();
        }

    }
}
