using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.Game.Extensions;
using Dos.Game.Util;

namespace Dos.DiscordBot
{
    public class GameProviderService
    {
        private readonly ConcurrentDictionary<ulong, DiscordGameWrapper> gamesByChannel =
            new ConcurrentDictionary<ulong, DiscordGameWrapper>();

        public async Task<Result<string>> JoinGameAsync(ISocketMessageChannel channel, IUser player)
        {
            if (gamesByChannel.TryGetValue(channel.Id, out var gameWrapper))
            {
                return await gameWrapper.JoinAsync(player);
            }

            gamesByChannel[channel.Id] = new DiscordGameWrapper(channel, player);
            return "You have created a game! Wait for other to join or start with `dos start`".ToSuccess();
        }

        public async Task<Result<string>> StartGameAsync(ISocketMessageChannel channel, IUser player)
        {
            if (!gamesByChannel.TryGetValue(channel.Id, out var gameWrapper))
                return "Sorry, but there's no game in this channel. If you want to create one, use `dos join`".ToFail();

            var result = await gameWrapper.StartAsync(player);
            if (!result.IsSuccess)
            {
                return result;
            }

            gamesByChannel.TryRemove(channel.Id, out _);
            return $"**TODO - game started and ended**".ToSuccess();
        }
    }
}
