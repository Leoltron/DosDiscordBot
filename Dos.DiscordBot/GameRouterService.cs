using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.Utils;
using Serilog;

namespace Dos.DiscordBot
{
    public class GameRouterService
    {
        private readonly ConcurrentDictionary<ulong, DiscordDosGame> gamesByChannel =
            new ConcurrentDictionary<ulong, DiscordDosGame>();

        private readonly ILogger logger;

        public GameRouterService(ILogger logger) => this.logger = logger;

        public async Task<Result> JoinGameAsync(IGuild guild, ISocketMessageChannel channel, IUser player)
        {
            if (gamesByChannel.TryGetValue(channel.Id, out var gameWrapper) && !gameWrapper.IsFinished)
                return await gameWrapper.JoinAsync(player);

            gamesByChannel[channel.Id] = new DiscordDosGame(channel, player, logger, guild.Name);
            return Result.Success("You have created a game! Wait for others to join or start with `dos start`");
        }

        public DiscordDosGame TryFindGameByChannel(ISocketMessageChannel channel)
        {
            var game = gamesByChannel.GetValueOrDefault(channel.Id);

            if (game == null || !game.IsFinished)
                return game;

            gamesByChannel.Remove(channel.Id, out _);
            return null;
        }

        public DiscordDosGame TryDeleteGame(ISocketMessageChannel channel)
        {
            gamesByChannel.TryRemove(channel.Id, out var game);
            return game;
        }

        public Result Quit(ISocketMessageChannel channel, IUser user)
        {
            var game = gamesByChannel.GetValueOrDefault(channel.Id);

            if (game == null)
                return Result.Fail();

            if (!game.Players.ContainsKey(user.Id))
                return Result.Fail();

            gamesByChannel.Remove(channel.Id, out _);
            return Result.Success("Game has been ended.");
        }
    }
}
