using System;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.DiscordBot.Util;

namespace Dos.DiscordBot.Attributes
{
    public class AdminOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services)
            => Task.FromResult(context.User.DiscordTag() == DosBot.AdminDiscordTag
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError(string.Empty));
    }
}
