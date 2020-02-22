using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;
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
                return Path.Combine(Folder, CardPrefix + "Wild_2.png");

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
            var trashCan = new DisposableList();

            using var images = new MagickImageCollection();
            foreach (var image in imageEnumerable)
            {
                if (separator != null)
                {
                    var separatorClone = separator.Clone();
                    images.Add(separatorClone);
                    trashCan.Add(separatorClone);
                    totalWidth += separatorWidth;
                }
                else
                {
                    separator = new MagickImage(MagickColors.Transparent, separatorWidth, image.Height);
                    trashCan.Add(separator);
                }

                images.Add(image);
                trashCan.Add(image);
                totalWidth += image.Width;
            }

            if (totalWidth < minWidth)
            {
                var filler = new MagickImage(MagickColors.Transparent, minWidth - totalWidth,
                                             separator?.Height ?? 20);
                images.Add(filler);
                trashCan.Add(filler);
            }

            using (trashCan)
            {
                using var result = images.AppendHorizontally();
                var stream = new MemoryStream();
                result.Write(stream, MagickFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        public static MagickImage Stack(this IEnumerable<Card> cards) =>
            cards.Select(ToImage).ToList().Stack();

        public static MagickImage Stack(this IList<MagickImage> images)
        {
            if (images.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(images), "Expected list with at least 1 element");

            if (images.Count == 1)
                return images[0];

            using var imageCollection = new MagickImageCollection();
            imageCollection.Add(images[0]);
            var xOffset = images[0].Width / 2;

            foreach (var image in images.Skip(1))
            {
                image.Page = new MagickGeometry(xOffset, 0, 100, 100);
                imageCollection.Add(image);
                xOffset += image.Width / 2;
            }

            using var mosaic = imageCollection.Mosaic();
            return new MagickImage(mosaic);
        }
    }
}
