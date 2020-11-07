namespace Dos.DiscordBot.Util
{
    public static class CardDisplayStyleExtensions
    {
        public static bool IsImage(this CardDisplayStyle style) => style != CardDisplayStyle.Text;
        public static bool IsText(this CardDisplayStyle style) => style != CardDisplayStyle.Image;
    }
}
