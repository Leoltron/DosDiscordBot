using System.Collections.Generic;
using System.Linq;
using Discord;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;
using Color = System.Drawing.Color;

namespace Dos.DiscordBot.Util
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithCardsImage(this EmbedBuilder builder, IList<Card> cards, bool addPlus) => 
            builder.WithImageUrl(cards.ToImageUrl(addPlus)).WithMostFrequentColor(cards);
        
        public static EmbedBuilder WithCardsImage(this EmbedBuilder builder, IList<List<Card>> cards, bool addPlus) => 
            builder.WithImageUrl(cards.ToImageUrl(addPlus)).WithMostFrequentColor(cards.SelectMany(c => c));

        public static EmbedBuilder WithMostFrequentColor(this EmbedBuilder builder, IEnumerable<Card> cards)
        {
            var color = cards.GroupBy(c => c.Color).WithMax(g => g.Count())?.Key.ToColor() ??
                        Color.Transparent;
            return builder.WithColor((uint) (color.ToArgb() & 0xFFFFFF));
        }
        
    }
}
