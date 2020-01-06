using System;

namespace Dos.Game.Model
{
    [Flags]
    public enum CardColor : byte
    {
        Red = 0b0001,
        Green = 0b0010,
        Yellow = 0b0100,
        Blue = 0b1000,
        Wild = 0b1111
    }
}
