using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class ColorExtensions
    {
        public static bool Matches(this CardColor one, CardColor other) => (one & other) != 0;

        public static string ShortName(this CardColor color)
        {
            return color switch
            {
                CardColor.Red => "R",
                CardColor.Green => "G",
                CardColor.Yellow => "Y",
                CardColor.Blue => "B",
                CardColor.Wild => "W",
                _ => color.ToString().Substring(0, 1)
            };
        }
    }
}
