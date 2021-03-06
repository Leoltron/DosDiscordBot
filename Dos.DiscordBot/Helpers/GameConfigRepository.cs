using System;
using System.Linq;
using System.Threading.Tasks;
using Dos.Database;
using Dos.Database.Models;
using Dos.Utils;

namespace Dos.DiscordBot.Helpers
{
    public class GameConfigRepository : IGameConfigRepository
    {
        public async Task<Result<BotGameConfig>> TryGetConfigAsync(ulong serverId, TimeSpan waitTime)
        {
            var task = GetConfigOrDefaultAsync(serverId);
            await Task.WhenAny(task, Task.Delay(waitTime));
            return task.ToResult();
        }

        public async Task<BotGameConfig> GetConfigOrDefaultAsync(ulong serverId) =>
            (await GetGuildConfigAsync(serverId))?.Config ?? new BotGameConfig();

        public async Task UpdateGameConfigAsync(ulong serverId, BotGameConfig config)
        {
            await using var dbContext = new DosDbContext();
            var guildConfig = await GetGuildConfigAsync(dbContext, serverId);
            if (guildConfig == null)
            {
                guildConfig = new GuildConfig
                {
                    GuildId = serverId
                };
                await dbContext.AddAsync(guildConfig);
            }
            guildConfig.Config = config;
            await dbContext.SaveChangesAsync();
        }

        private static async Task<GuildConfig> GetGuildConfigAsync(ulong serverId)
        {
            await using var context = new DosDbContext();
            return await GetGuildConfigAsync(context, serverId);
        }

        private static async Task<GuildConfig> GetGuildConfigAsync(DosDbContext context, ulong serverId)
        {
            return await context.GuildConfig.FirstOrDefaultAsync(c => c.GuildId == serverId);
        }
    }
}
