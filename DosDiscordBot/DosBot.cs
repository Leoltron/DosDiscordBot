using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DosDiscordBot
{
    public class DosBot
    {
        private DiscordSocketClient client;
        private Random rand = new Random();

        public async Task StartAsync()
        {
            client = new DiscordSocketClient();

            client.Log += Log;
            client.MessageReceived += MessageReceived;

            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.
            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        
        private static readonly Regex unoRegex = new Regex("(?:\\s|^)uno(?:\\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.Id != client.CurrentUser.Id && unoRegex.IsMatch(message.Content) && rand.Next(100) < 5)
            {
                await message.Channel.SendMessageAsync("UNO is good, but did you try playing DOS?");
            }
        }
    }
}
