using System.Threading.Tasks;
using Discord.Commands;

namespace Dos.DiscordBot.Helpers
{
    public interface IGameConfigHelper
    {
        Task SendGameConfigAsync(DiscordDosGame game);
        Task SendServerDefaultGameConfigAsync(SocketCommandContext context);
        Task SetGameConfigAsync(SocketCommandContext context, string key, string value);
        Task SetServerDefaultGameConfigAsync(SocketCommandContext context, string key, string value);
    }
}
