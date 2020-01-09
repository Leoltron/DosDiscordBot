using System;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot
{
    public class RunningGameRequiredAttribute : PreconditionAttribute
    {
        private const string GameNotFoundMessage = "Sorry, but there's no game in this channel. " +
                                                   "If you want to create one, use `dos join`";

        private static readonly PreconditionResult GameNotFoundResult = PreconditionResult.FromError(GameNotFoundMessage);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services) =>
            Task.FromResult(context.GetGame() != null ? PreconditionResult.FromSuccess() : GameNotFoundResult);
    }
}
