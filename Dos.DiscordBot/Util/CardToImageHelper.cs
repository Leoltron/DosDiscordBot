using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Model;
using ImageMagick;

namespace Dos.DiscordBot.Util
{
    public static class CardToImageHelper
    {
        private const string Folder = "Resources";
        private const string CardPrefix = "DOS_Card_";
        private static readonly string Back = Path.Combine(Folder, CardPrefix + "Back.png");
        private static readonly string Default = Back;

        public static readonly string PlusPath = Path.Combine(Folder, "Plus.png");

        public static string ToImagePath(this Card card)
        {
            if (card == Card.WildDos)
            {
                return Path.Combine(Folder, CardPrefix + "Wild_2.png");
            }

            if (card.Color == CardColor.Wild)
                return Default;

            var color = card.Color.ToString();

            var value = card.Value.Name();

            return Path.Combine(Folder, color, $"{CardPrefix}{color}_{value}.png");
        }

        public static MagickImage ToImage(this Card card) => card.ToImagePath().ToImage();
        public static MagickImage ToImage(this string path) => new MagickImage(path);

        public static Stream JoinImages(this IEnumerable<Card> cards) => cards.Select(ToImage).JoinImages(10, 1173);
        public static Stream JoinImages(this IEnumerable<string> paths) => paths.Select(ToImage).JoinImages(10, 1173);

        public static Stream JoinImages(this IEnumerable<MagickImage> imageEnumerable, int separatorWidth,
                                        int minWidth = 0)
        {
            MagickImage separator = null;
            var totalWidth = 0;

            using (var images = new MagickImageCollection())
            {
                foreach (var image in imageEnumerable)
                {
                    if (separator != null)
                    {
                        images.Add(separator.Clone());
                        totalWidth += separatorWidth;
                    }
                    else
                    {
                        separator = new MagickImage(MagickColors.Transparent, separatorWidth, image.Height);
                    }

                    images.Add(image);
                    totalWidth += image.Width;
                }

                if (totalWidth < minWidth)
                {
                    images.Add(
                        new MagickImage(MagickColors.Transparent, minWidth - totalWidth, separator?.Height ?? 20));
                }

                using (var result = images.AppendHorizontally())
                {
                    var stream = new MemoryStream(2<<20);
                    result.Write(stream, MagickFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
        }
    }
}
