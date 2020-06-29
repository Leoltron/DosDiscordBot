using System;
using System.Configuration;
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
        public const ulong AdminId = 217276381778804736;
        public const string SupportLink = "https://discord.gg/P3ycQAM";
        public const string InviteLink = "https://discordapp.com/api/oauth2/authorize?client_id=660931324642590720&permissions=52224&scope=bot";
        public const string RepoLink = "https://github.com/Leoltron/DosDiscordBot";

        private const string VersionStatus = "v1.3.0 (29.06.2020)";

        private readonly ILogger logger = new LoggerConfiguration()
                                         .WriteTo.Console(LogEventLevel.Information)
                                         .WriteTo.File("logs/bot-.log", rollingInterval: RollingInterval.Day)
                                         .CreateLogger();

        private DiscordSocketClient client;
        private CommandHandler commandHandler;

        public async Task StartAsync(string token)
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000,
                ExclusiveBulkDelete = true
            });
            await client.SetGameAsync(VersionStatus);

            client.Log += Log;
            var commandService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async
            });
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
