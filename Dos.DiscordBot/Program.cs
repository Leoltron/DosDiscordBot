namespace Dos.DiscordBot
{
    internal static class Program
    {
        public static void Main()
        {
            new DosBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
