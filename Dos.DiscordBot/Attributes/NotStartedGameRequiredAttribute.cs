using System;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Attributes
{
    public class NotStartedGameRequiredAttribute : CreatedGameRequiredAttribute
    {
        private const string AlreadyStartedMessage = "Game has been started already.";

        private static readonly PreconditionResult GameAlreadyStartedResult =
            PreconditionResult.FromError(AlreadyStartedMessage);

        public override async Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var result = await base.CheckPermissionsAsync(context, command, services);
            if (!result.IsSuccess)
                return result;

            var game = context.GetGame();
            return game != null && !game.IsGameStarted ? PreconditionResult.FromSuccess() : GameAlreadyStartedResult;
        }
    }
}
