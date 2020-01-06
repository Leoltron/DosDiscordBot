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

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services)
        {
            if(context.GetGame() != null)
                return PreconditionResult.FromSuccess();

            await context.Channel.SendMessageAsync(GameNotFoundMessage);
            return GameNotFoundResult;
        }
    }
}
