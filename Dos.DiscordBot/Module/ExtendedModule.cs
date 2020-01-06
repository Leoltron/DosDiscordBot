using System.Threading.Tasks;
using Discord.Commands;
using Dos.Utils;

namespace Dos.DiscordBot.Module
{
    public abstract class ExtendedModule : ModuleBase<SocketCommandContext>
    {
        public async Task ReplyIfHasMessageAsync(Result result)
        {
            if (result.HasMessage)
                await ReplyAsync(result.Message);
        }
    }
}
