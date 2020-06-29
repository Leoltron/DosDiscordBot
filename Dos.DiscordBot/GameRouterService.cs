using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.Database.Models;
using Dos.DiscordBot.Helpers;
using Dos.Utils;
using Serilog;

namespace Dos.DiscordBot
{
    public class GameRouterService
    {
        private static readonly TimeSpan InactiveGameTimeout = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<ulong, DiscordDosGame> gamesByChannel =
            new ConcurrentDictionary<ulong, DiscordDosGame>();

        private readonly ILogger logger;
        private readonly IGameConfigRepository configRepository;

        public GameRouterService(ILogger logger, IGameConfigRepository configRepository)
        {
            this.logger = logger;
            this.configRepository = configRepository;
        }

        public bool PreventStartNewGames { get; private set; }

        public int GetActiveGamesCount => gamesByChannel.Count(pair => !pair.Value.IsFinished);

        public event Action LastGameEnded;

        public async Task<Result> JoinGameAsync(IGuild guild, ISocketMessageChannel channel, IUser player)
        {
            if (gamesByChannel.TryGetValue(channel.Id, out var gameWrapper) && !gameWrapper.IsFinished)
                return await gameWrapper.JoinAsync(player).ConfigureAwait(false);

            if (PreventStartNewGames)
                return Result.Fail("Bot is waiting for all active games to end to restart.");

            var createdGame = gamesByChannel[channel.Id] = await CreateNewGameAsync(guild, channel, player);
            DeleteIfNoActivity(channel, InactiveGameTimeout);
            return Result.Success(
                "You have created a game with following configuration (change with `dos config <key> <value>`): \n" +
                createdGame.Config.ToDiscordTable() +
                "\n Wait for others to join or start with `dos start`");
        }

        private async Task<DiscordDosGame> CreateNewGameAsync(IGuild guild, ISocketMessageChannel channel, IUser player)
        {
            var config = guild == null? new BotGameConfig() : await configRepository.GetConfigOrDefaultAsync(guild.Id);
            var game = new DiscordDosGame(channel, player, logger, config, guild?.Name ?? "DM");
            game.Finished += () => TryDeleteGame(channel);
            return game;
        }

        public DiscordDosGame TryFindGameByChannel(ISocketMessageChannel channel)
        {
            var game = gamesByChannel.GetValueOrDefault(channel.Id);

            if (game == null || !game.IsFinished)
                return game;

            TryDeleteGame(channel);
            return null;
        }

        public void TryDeleteGame(ISocketMessageChannel channel)
        {
            gamesByChannel.TryRemove(channel.Id, out _);

            if (GetActiveGamesCount == 0)
                LastGameEnded?.Invoke();
        }

        private async void DeleteIfNoActivity(ISocketMessageChannel channel, TimeSpan timeout)
        {
            var expectedCreateDate = TryFindGameByChannel(channel)?.CreateDate;
            await Task.Delay(timeout).ConfigureAwait(false);
            var game = TryFindGameByChannel(channel);
            if (game == null || game.CreateDate != expectedCreateDate || game.Players.Count > 1 || game.IsGameStarted)
                return;

            TryDeleteGame(channel);
            await channel.SendMessageAsync("The game has been cancelled due to inactivity.").ConfigureAwait(false);
        }

        public Result Quit(ISocketMessageChannel channel, IUser user)
        {
            var game = gamesByChannel.GetValueOrDefault(channel.Id);

            if (game == null)
                return Result.Fail();

            if (!game.IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            if (game.IsGameStarted)
            {
                game.Game.Quit(player);
            }
            else if (game.Players.Count < 3 || user.Id == game.Owner.Id)
            {
                TryDeleteGame(channel);
                return Result.Success("Game has been cancelled.");
            }

            game.Players.Remove(player);
            game.IdToUserPlayers.Remove(user.Id);

            return Result.Success();
        }

        public void NoNewGames()
        {
            PreventStartNewGames = true;
        }

        public IEnumerable<DiscordDosGameInfo>
            GetRunningGamesInfo() => gamesByChannel.Select(pair => pair.Value)
                                                   .Select(g => g.Info);
    }
}
