using System.Drawing;
using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class ColorExtensions
    {
        public static bool Matches(this CardColor one, CardColor other) => (one & other) != 0;

        public static string ShortName(this CardColor color) =>
            color switch
            {
                CardColor.Red => "R",
                CardColor.Green => "G",
                CardColor.Yellow => "Y",
                CardColor.Blue => "B",
                CardColor.Wild => "W",
                _ => color.ToString().Substring(0, 1)
            };

        public static Color ToColor(this CardColor color) => color switch
        {
            CardColor.Red => Color.FromArgb(231, 4, 23),
            CardColor.Green => Color.FromArgb(69, 163, 66),
            CardColor.Yellow => Color.FromArgb(255, 209, 17),
            CardColor.Blue => Color.FromArgb(9, 112, 181),
            CardColor.Wild => Color.Black,
            _ => Color.Transparent
        };
    }
}
