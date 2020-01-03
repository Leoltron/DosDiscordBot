using System;

namespace Dos.Game.Model
{
    [Flags]
    public enum CardColor : byte
    {
        Blue = 0b0001,
        Green = 0b0010,
        Red = 0b0100,
        Yellow = 0b1000,
        Wild = 0b1111
    }

    public static class ColorExtensions
    {
        public static bool Matches(this CardColor one, CardColor other) => (one & other) != 0;
    }
}
