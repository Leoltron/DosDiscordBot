using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Dos.DiscordBot.Attributes
{
    public class AdminOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services)
            => Task.FromResult(context.User.Id == DosBot.AdminId
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError(string.Empty));
    }
}
