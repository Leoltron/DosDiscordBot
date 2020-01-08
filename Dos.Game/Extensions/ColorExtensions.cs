using System;
using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class ColorExtensions
    {
        public static bool Matches(this CardColor one, CardColor other) => (one & other) != 0;

        public static string ShortName(this CardColor color)
        {
            switch (color)
            {
                case CardColor.Red:
                    return "R";
                case CardColor.Green:
                    return "G";
                case CardColor.Yellow:
                    return "Y";
                case CardColor.Blue:
                    return "B";
                case CardColor.Wild:
                    return "W";
                default:
                    return color.ToString().Substring(0, 1);
            }
        }
    }
}
