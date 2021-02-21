using System;
using Dos.Game.Model;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Dos.Game
{
    public class CardJsonConverter : JsonConverter<Card>
    {
        public override void WriteJson(JsonWriter writer, Card card, JsonSerializer serializer)
        {
            writer.WriteValue(card.ShortName);
        }

        public override Card ReadJson(JsonReader reader, Type objectType, Card existingValue, bool hasExistingValue,
                                      JsonSerializer serializer) =>
            CardParser.ParseShortCard(((string) reader.Value)?.ToLowerInvariant());
    }

    public class NullableCardJsonConverter : JsonConverter<Card?>
    {
        public override void WriteJson(JsonWriter writer, Card? card, JsonSerializer serializer)
        {
            if (card.HasValue)
                writer.WriteValue(card.Value.ShortName);
            else
                writer.WriteNull();
        }

        public override Card? ReadJson(JsonReader reader, Type objectType, Card? existingValue, bool hasExistingValue,
                                       JsonSerializer serializer) =>
            CardParser.TryParseShortCard(((string) reader.Value)?.ToLowerInvariant(), out var card)
                ? card
                : (Card?) null;
    }
}
