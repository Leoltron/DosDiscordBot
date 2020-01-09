using System;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Attributes
{
    public class StartedGameRequiredAttribute : CreatedGameRequiredAttribute
    {
        private const string GameNotStartedMessage = "Game has not been started yet.";

        private static readonly PreconditionResult GameNotStartedResult =
            PreconditionResult.FromError(GameNotStartedMessage);

        public override async Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var result = await base.CheckPermissionsAsync(context, command, services);
            if (!result.IsSuccess)
                return result;

            var game = context.GetGame();
            return game != null && game.IsGameStarted ? PreconditionResult.FromSuccess() : GameNotStartedResult;
        }
    }
}
