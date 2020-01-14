using System;
using System.IO;
using Dos.Utils;

namespace Dos.DiscordBot
{
    internal static class Program
    {
        private const string TokenFilePath = "token.txt";

        public static void Main()
        {
            new DosBot().StartAsync(TryFindToken()).GetAwaiter().GetResult();
        }

        private static string TryFindToken()
        {
            var token = Environment.GetEnvironmentVariable("DiscordToken");
            if (!token.IsNullOrWhiteSpace())
            {
                Console.WriteLine("Found token in environment variable");
                return token;
            }

            if (File.Exists(TokenFilePath))
            {
                token = File.ReadAllText(TokenFilePath);
                Console.WriteLine("Found token in token.txt file");
            }

            return token;
        }
    }
}
