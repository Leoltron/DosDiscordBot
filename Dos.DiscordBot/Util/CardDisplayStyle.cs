using System;

namespace Dos.DiscordBot.Util
{
    [Flags]
    public enum CardDisplayStyle : byte
    {
        Text = 1,
        Image = 2,
        Hybrid = Text | Image
    }
}
