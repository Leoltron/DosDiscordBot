using System;
using System.Linq;
using System.Threading.Tasks;
using Dos.Database;
using Dos.Database.Models;
using Dos.Game.Extensions;
using Dos.Game.Logging;
using Dos.Game.Players;

namespace Dos.DiscordBot.GameLogging
{
    public class DbGameLogger : GameLogger
    {
        private readonly DiscordDosGame discordDosGame;
        private Guid replayId;

        public DbGameLogger(DiscordDosGame game) : base(game.Game)
        {
            discordDosGame = game;
            Init(game);
            game.Game.Events.Finished += OnGameFinished;
        }

        private void Init(DiscordDosGame discordGame)
        {
            var replay = new Replay
            {
                GameStartDate = DateTime.UtcNow,
                Players =
                    discordGame.Players.Select(p => new ReplayPlayer {OrderId = p.OrderId, PlayerName = p.Name})
                               .ToList(),
                IsOngoing = true,
                ChannelTitle = discordGame.Info.Channel.Name,
                GuildTitle = discordGame.Info.ServerName,
                IsPublic = discordGame.Config.PublishReplays
            };
            using var context = new DosDbContext();
            context.Replay.Add(replay);
            context.SaveChanges();

            replayId = replay.ReplayId;
        }

        private async void OnGameFinished()
        {
            if (game.Players.Any(p => p.State == PlayerState.Out || p.IsActive()) && Events.Count > 10)
            {
                await FinishSavingReplay();
                await discordDosGame.SendToChannelAsync(
                    $"Replay is available at http://localhost:5000/replay/{replayId}");
            }
            else
            {
                await DeleteReplay();
            }
        }

        private async Task DeleteReplay()
        {
            await using var context = new DosDbContext();
            var replay = await context.Replay.FindAsync(replayId);
            if (replay != null)
            {
                context.Replay.Remove(replay);
                await context.SaveChangesAsync();
            }
        }

        private async Task FinishSavingReplay()
        {
            await using var context = new DosDbContext();
            var replay = await context.Replay.FindAsync(replayId);
            if (replay != null)
            {
                replay.IsOngoing = false;
                await context.SaveChangesAsync();
            }
        }

        protected override void AppendEvent(GameLogEvent gameLogEvent, GameSnapshot gameSnapshot)
        {
            base.AppendEvent(gameLogEvent, gameSnapshot);
            using var context = new DosDbContext();
            context.ReplayMove.Add(new ReplayMove
            {
                TargetPlayer = gameLogEvent.TargetPlayer,
                SourcePlayer = gameLogEvent.SourcePlayer,
                Cards = gameLogEvent.Cards,
                EventType = gameLogEvent.EventType,
                ReplayId = replayId
            });
            context.ReplaySnapshot.Add(new ReplaySnapshot
            {
                CenterRow = gameSnapshot.CenterRow,
                PlayerHands = gameSnapshot.PlayerHands,
                ReplayId = replayId,
                CurrentPlayerId = gameSnapshot.CurrentPlayerId,
            });
            context.SaveChanges();
        }

        public static void LogGame(DiscordDosGame game)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new DbGameLogger(game);
        }
    }
}
