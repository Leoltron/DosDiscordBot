using System;
using Discord;
using Discord.Commands;
using Dos.DiscordBot.Commands;
using Serilog.Events;

namespace Dos.DiscordBot.Util
{
    public static class Extensions
    {
        public static DiscordDosGame GetGame(this ICommandContext context) => (context as DosCommandContext)?.DosGame;

        public static LogEventLevel ToLogLevel(this LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogEventLevel.Fatal;
                case LogSeverity.Error:
                    return LogEventLevel.Error;
                case LogSeverity.Warning:
                    return LogEventLevel.Warning;
                case LogSeverity.Info:
                    return LogEventLevel.Information;
                case LogSeverity.Verbose:
                    return LogEventLevel.Debug;
                case LogSeverity.Debug:
                    return LogEventLevel.Verbose;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }
    }
}
