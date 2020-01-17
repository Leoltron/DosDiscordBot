using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Dos.DiscordBot.Commands;
using Dos.DiscordBot.Util;
using Serilog;
using Serilog.Events;

namespace Dos.DiscordBot
{
    public class DosBot
    {
        private readonly ILogger logger = new LoggerConfiguration()
                                         .WriteTo.Console(LogEventLevel.Information)
                                         .WriteTo.File("logs/bot-.log", rollingInterval: RollingInterval.Day)
                                         .CreateLogger();

        private DiscordSocketClient client;
        private CommandHandler commandHandler;

        public const string AdminDiscordTag = "Leoltron#9479";

        public async Task StartAsync(string token)
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            client.Log += Log;
            var commandService = new CommandService(new CommandServiceConfig {LogLevel = LogSeverity.Verbose});
            commandHandler = new CommandHandler(client, commandService, ServiceProviderBuilder.BuildProvider(logger));

            await commandHandler.InstallCommandsAsync();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            logger.Write(msg.Severity.ToLogLevel(), msg.Exception, $"[{msg.Source}] {msg.Message}");
            return Task.CompletedTask;
        }
    }
}
