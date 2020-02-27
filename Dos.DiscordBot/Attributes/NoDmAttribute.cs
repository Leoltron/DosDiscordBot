using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Dos.DiscordBot.Attributes
{
    public class NoDmAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services)
            => Task.FromResult(context.Message.Channel is IPrivateChannel
                                   ? PreconditionResult.FromError("Sorry, but this command cannot be executed in DM")
                                   : PreconditionResult.FromSuccess());
    }
}
