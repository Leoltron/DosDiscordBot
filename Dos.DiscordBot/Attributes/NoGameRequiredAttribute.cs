using System;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Attributes
{
    public class NoGameRequiredAttribute : PreconditionAttribute
    {
        private const string GameExistsMessage = "There is a game in this channel. Finish it or move to other channel.";

        private static readonly PreconditionResult GameExistsResult = PreconditionResult.FromError(GameExistsMessage);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services) =>
            Task.FromResult(context.GetGame() != null ? GameExistsResult : PreconditionResult.FromSuccess());
    }
}
