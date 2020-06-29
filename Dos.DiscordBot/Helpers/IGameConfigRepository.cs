using System;
using System.Threading.Tasks;
using Dos.Database.Models;
using Dos.Utils;

namespace Dos.DiscordBot.Helpers
{
    public interface IGameConfigRepository
    {
        Task<Result<BotGameConfig>> TryGetConfigAsync(ulong serverId, TimeSpan waitTime);
        Task<BotGameConfig> GetConfigOrDefaultAsync(ulong serverId);
        Task UpdateGameConfigAsync(ulong serverId, BotGameConfig config);
    }
}
