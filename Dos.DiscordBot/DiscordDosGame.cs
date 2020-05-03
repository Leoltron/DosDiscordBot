using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.DiscordBot.Util;
using Dos.Game;
using Dos.Game.Deck;
using Dos.Game.Events;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Game.State;
using Dos.Utils;
using Serilog;

namespace Dos.DiscordBot
{
    public class DiscordDosGame
    {
        private readonly ILogger gameLogger;
        private readonly ILogger mainLogger;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public readonly IUser Owner;
        public readonly List<Player> Players = new List<Player>();

        private Card? selectedCenterRowCard;
        private DateTime startTime;

        public DiscordDosGame(ISocketMessageChannel channel, IUser owner, ILogger mainLogger, string serverName)
        {
            this.mainLogger = mainLogger;
            var logFileName = $"{DateTime.Now:yyyy-MM-dd_HHmmssZ}__{serverName}#{channel.Name}"
               .Replace(Path.GetInvalidFileNameChars());
            gameLogger = new LoggerConfiguration()
                        .WriteTo.File($"logs/games/{logFileName}.log")
                        .CreateLogger();
            Info = new DiscordDosGameInfo(serverName, CreateDate, channel, owner);
            AddUserPlayer(owner);
            Owner = owner;

            LogInfo($"{owner.Username} have created the game");
        }

        public DiscordDosGameInfo Info { get; }

        public DateTime CreateDate { get; } = DateTime.UtcNow;

        public Dictionary<ulong, DiscordUserPlayer> IdToUserPlayers { get; } =
            new Dictionary<ulong, DiscordUserPlayer>();

        public DosGame Game { get; private set; }
        public bool IsGameStarted => Game != null;

        public bool IsFinished => IsGameStarted && Game.CurrentState.IsFinished;

        public BotGameConfig Config { get; } = new BotGameConfig {StartingPlayer = 0};

        private void AddUserPlayer(IUser user)
        {
            var player = new DiscordUserPlayer(Players.Count, user);
            Players.Add(player);
            IdToUserPlayers[user.Id] = player;
        }

        private AiPlayer AddAiPlayer()
        {
            var player = new RandomAiPlayer(Players.Count);
            Players.Add(player);
            LogInfo($"Added AI player {player.Name} ({player.GetType().Name})");
            return player;
        }

        public event Action Finished;

        private void LogInfo(string message)
        {
            mainLogger.Information($"[{Info.ServerName} - #{Info.Channel.Name}] [game] {message}");
        }

        public async Task<Result> JoinAsync(IUser user)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (IdToUserPlayers.ContainsKey(user.Id))
                    return Result.Fail("You have already joined this game");

                AddUserPlayer(user);

                return Result.Success($"{user.Username} has joined the game!");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private const int MaxBotPlayers = 2;

        public async Task<Result> AddBotAsync(IUser user)
        {
            if (Owner.Id != user.Id)
                return Result.Fail($"Only owner of this game (**{Owner?.Username}**) can add bots");

            if (Players.Count(p => p.IsAi) >= MaxBotPlayers)
            {
                return Result.Fail($"Sorry, but I cannot add more than {MaxBotPlayers} bot players");
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                var player = AddAiPlayer();

                return Result.Success($"**{player.Name}** has joined the game!");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }


        public async Task<Result> StartAsync(IUser player)
        {
            if (!IdToUserPlayers.ContainsKey(player.Id))
                return Result.Fail();

            if (IsGameStarted)
                return Result.Fail("Game has already started");

            await semaphoreSlim.WaitAsync();
            try
            {
                if (Owner.Id != player.Id)
                    return Result.Fail($"Only owner of this game (**{Owner?.Username}**) can start it");

                if (Players.Count == 1)
                {
                    var newPlayer = AddAiPlayer();
                    await Info.Channel.SendMessageAsync($"Not enough players, **{newPlayer.Name}** joins the game!");
                }

                Game = new DosGame(new ShufflingDealer(Decks.Classic.Times(Config.Decks).ToArray()),
                                   Players.ToArray(),
                                   Config);

                BindGameEvents();
                Game.Start();
                Game.Events.PlayerReceivedCards += OnPlayerReceivedCards;

                await Task.WhenAll(Players.Select(SendHandTo));
                await SendTableToChannel(false);
                startTime = DateTime.Now;

                return Result.Success();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private void BindGameEvents()
        {
            Game.Events.PrivateLog += e =>
            {
                LogInfo(e.Message);
                gameLogger.Information(e.Message);
            };
            Game.Events.PublicLog += e => Info.Channel.SendMessageAsync(e.Message).Wait();
            Game.Events.Finished += OnFinished;
            Game.Events.PlayerSwitched += OnPlayerSwitched;
            Game.Events.PlayersSwappedHands += e => Task.WaitAll(SendHandTo(e.Player), SendHandTo(e.Target));

            Game.Events.WentOut += e => SendToChannel(
                $"{e.Player.Name} has no more cards! They finished in Rank #{e.Player.ScoreBoardPosition}! :tada:");
        }

        private IEnumerable<string> GetScoreboardLines()
        {
            var linesForPlayersWhoWentOut =
                Players.Where(p => p.State == PlayerState.Out)
                       .OrderBy(p => p.ScoreBoardPosition)
                       .Select(p => $"#{p.ScoreBoardPosition}: {p.Name}");

            var linesForActivePlayers =
                Players.Where(p => p.IsActive())
                       .OrderBy(p => p.ScoreBoardPosition)
                       .Select(p => $"#{p.ScoreBoardPosition}: {p.Name} - {PlayerScoreString(p)}");


            return linesForPlayersWhoWentOut.Concat(linesForActivePlayers);
        }

        private string PlayerScoreString(Player player)
        {
            return Config.CardCountRanking
                ? player.Hand.Count.Pluralize("card", "cards")
                : player.Hand.Sum(c => c.Points).Pluralize("point", "points");
        }

        private void OnFinished()
        {
            if (Players.Any(p => p.State == PlayerState.Out || p.IsActive()))
            {
                SendToChannel("The game has finished with the following results:\n" +
                              string.Join("\n", GetScoreboardLines()));
            }

            var time = DateTime.Now - startTime;
            var timeString = time > TimeSpan.FromDays(1) ? "**More than a day**" : $@"{time:hh\:mm\:ss\.fff}";
            SendToChannel($"`The game lasted {timeString}`");

            Finished?.Invoke();
        }

        public void SendToChannel(string message)
        {
            Info.Channel.SendMessageAsync(message).Wait();
        }

        private void OnPlayerReceivedCards(PlayerReceivedCardsEvent e)
        {
            if (e.Player is DiscordUserPlayer dPlayer && e.Cards.Any())
                dPlayer.User.SendCardsAsync(e.Cards, Config.UseImages, true).Wait();
        }

        private void OnPlayerSwitched(PlayerSwitchedEvent @event)
        {
            selectedCenterRowCard = null;
            Task.WaitAll(SendHandTo(@event.NextPlayer), SendTableToChannel(false));
        }

        public Task SendTableToChannel(bool addPlayersStats) => SendTableToChannel(addPlayersStats, Config.UseImages);

        private async Task SendTableToChannel(bool addPlayersStats, bool useImages)
        {
            if (!IsGameStarted)
            {
                await Info.Channel.SendMessageAsync("Game has not started yet");
            }
            else
            {
                if (addPlayersStats)
                {
                    var messageBuilder = new StringBuilder();
                    foreach (var player in Game.Players.Where(p => p.IsActive()))
                        messageBuilder.Append($"{player} - {player.Hand.Count.Pluralize("card", "cards")}\n");

                    await Info.Channel.SendMessageAsync(messageBuilder.ToString());
                }

                await Info.Channel.SendMessageAsync("**Center Row:**");
                if (useImages)
                    await Info.Channel.SendCardsAsync(
                        Game.CenterRow.Select((c, i) => Game.CenterRowAdditional[i].Prepend(c).ToList()));
                else
                    await Info.Channel.SendMessageAsync(string.Join("\n", Game.GameTableLines()));

                if (selectedCenterRowCard != null)
                    await Info.Channel.SendMessageAsync($"Selected **{selectedCenterRowCard.Value}**");

                await Info.Channel.SendMessageAsync($"Now it's **{Game.CurrentPlayer.Name}**'s turn");
            }
        }

        private async Task SendHandTo(Player player)
        {
            if (!(player is DiscordUserPlayer dPlayer))
                return;
            try
            {
                await dPlayer.User.SendCardsAsync(dPlayer.Hand, Config.UseImages);
            }
            catch (Exception e)
            {
                mainLogger.Warning(e, $"Failed to send hand to @{dPlayer.Name}");
                await Info.Channel.SendMessageAsync($"Failed to send hand to {dPlayer.User.Mention}." +
                                                    "Try check if DM is closed and use `dos hand` to try again.");
            }
        }

        public async Task<Result> MatchAsync(IUser user, string args)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                if (selectedCenterRowCard == null || args.Contains(" on ", StringComparison.InvariantCultureIgnoreCase))
                    return CardParser.ParseMatchCards(args)
                                     .IfSuccess(result => Game.MatchCenterRowCard(player,
                                                                                  result.Value.target,
                                                                                  result.Value.matchers));
                else
                    return CardParser.ParseCards(args)
                                     .IfSuccess(r => Game.MatchCenterRowCard(
                                                    player, selectedCenterRowCard.Value,
                                                    r.Value.ToArray()))
                                     .IfSuccess(r =>
                                      {
                                          selectedCenterRowCard = null;
                                          return r;
                                      });
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SelectAsync(IUser user, string args)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                if (Game.CurrentPlayer != player)
                    return Result.Fail("It's not your turn right now.");

                if (Game.CurrentState is AddingToCenterRowState)
                    return Result.Fail("You cannot match cards if you already started adding cards to the Center Row");

                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;

                if (matchResult.Value.IsEmpty())
                    return Result.Fail("If you want to select card, you need to tell me which one");

                var card = matchResult.Value.First();
                if (!Game.CenterRow.Contains(card))
                    return Result.Fail($"There's no **{card}** in the Center Row");

                selectedCenterRowCard = card;
                return Result.Success($"Selected **{card}**. Now use `dos match <card(s)>` to match it with something");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> AddToCenterRowAsync(IUser user, string args)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;
                else if (matchResult.Value.IsEmpty())
                    return Result.Fail("You have to put something there");
                else
                    return Game.AddCardToCenterRow(player, matchResult.Value.First());
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SendHandAsync(IUser user, bool reportToChannel)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                await SendHandTo(player);
                return Result.Success(reportToChannel ? "DM'd you your hand" : null);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> EndTurnAsync(IUser user)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.EndTurn(player);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> DrawAsync(IUser user)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.Draw(player);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> CallDosAsync(IUser user)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.CallDos(player);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> CalloutAsync(IUser user)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.Callout(player, Players.FirstOrDefault(p => p.CanBeCalledOut));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SwapAsync(IUser user, IUser target)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            if (!IdToUserPlayers.TryGetValue(target.Id, out var targetPlayer))
                return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.SwapWith(player, targetPlayer);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SwapAsync(IUser user, string targetName)
        {
            if (!IdToUserPlayers.TryGetValue(user.Id, out var player))
                return Result.Fail();

            var possibleTargets =
                Players
                   .Where(p => p.State == PlayerState.WaitingForTurn)
                   .Where(p => p.Name.StartsWith(targetName, StringComparison.InvariantCultureIgnoreCase))
                   .ToList();
            if (possibleTargets.IsEmpty())
                return Result.Fail($"Failed to find player with name starting with {targetName}. " +
                                   "Try use names from `dos table`");

            if (possibleTargets.Count > 1)
            {
                return Result.Fail($"Found more than one possible target with name starting with {targetName}. " +
                                   "Possible targets:\n" + "\n".Join(possibleTargets.Select(p => p.Name)));
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.SwapWith(player, possibleTargets.First());
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> StopAsync(IUser user)
        {
            if (!Config.AllowGameStop)
                return Result.Fail("Game stop is disabled for this game");


            if (Owner.Id != user.Id)
                return Result.Fail($"Only owner of this game (**{Owner?.Username}**) can stop it");

            await semaphoreSlim.WaitAsync();
            try
            {
                Game.SetFinished();
                return Result.Success();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
