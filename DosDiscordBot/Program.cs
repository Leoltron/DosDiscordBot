
namespace DosDiscordBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new DosBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}
