using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.Database.Models;
using Dos.DiscordBot.GameLogging;
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
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public readonly IUser Owner;
        public readonly List<Player> Players = new();

        private Card? selectedCenterRowCard;
        private DateTime? startTime;

        public DiscordDosGame(ISocketMessageChannel channel, IUser owner, ILogger mainLogger, BotGameConfig config,
                              string serverName)
        {
            this.mainLogger = mainLogger;
            var logFileName = $"{DateTime.Now:yyyy-MM-dd_HHmmssZ}__{serverName}#{channel.Name}"
               .Replace(Path.GetInvalidFileNameChars());
            gameLogger = new LoggerConfiguration()
                        .WriteTo.File($"logs/games/{logFileName}.log")
                        .CreateLogger();
            config.StartingPlayer = 0;
            Config = config;
            Info = new DiscordDosGameInfo(serverName, CreateDate, channel, owner);
            AddUserPlayer(owner);
            Owner = owner;

            LogInfo($"{owner.Username} have created the game");
        }

        public DiscordDosGameInfo Info { get; }

        public DateTime CreateDate { get; } = DateTime.UtcNow;

        public Dictionary<ulong, DiscordUserPlayer> IdToUserPlayers { get; } =
            new();

        public DosGame Game { get; private set; }
        public bool IsGameStarted => Game != null;

        public bool IsFinished => IsGameStarted && Game.CurrentState.IsFinished;

        public BotGameConfig Config { get; }

        public bool UseHybridCardDisplayStyle { get; set; } = false;

        public CardDisplayStyle CardDisplayStyle => UseHybridCardDisplayStyle ? CardDisplayStyle.Hybrid :
            Config.UseImages ? CardDisplayStyle.Image : CardDisplayStyle.Text;

        private void AddUserPlayer(IUser user)
        {
            var player = new DiscordUserPlayer(Players.Count, user);
            Players.Add(player);
            IdToUserPlayers[user.Id] = player;
        }

        private static readonly WeightConfig AiWeightConfig = new()
        {
            SingleMatchCoef = 3588,
            SingleColorMatchCoef = 6126,
            DoubleMatchCoef = 1418,
            DoubleColorMatchCoef = 8617,
        };

        private AiPlayer AddAiPlayer()
        {
            var player = new WeightAiPlayer(Players.Count, AiWeightConfig);
            Players.Add(player);
            LogInfo($"Added AI player {player.Name} ({player.GetType().Name})");
            return player;
        }

        public event Action Finished;

        private void LogInfo(string message)
        {
            mainLogger.Information("[{Server} - #{Channel}] [game] {Message}", Info.ServerName, Info.Channel.Name,
                                   message);
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

                if (Config.SaveReplays)
                    DbGameLogger.LogGame(this);
                BindGameEvents();
                Game.Start();
                Game.Events.PlayerReceivedCards += OnPlayerReceivedCards;
                startTime = DateTime.UtcNow;
/*
                await Task.WhenAll(Players.Select(SendHandTo));
                await SendTableToChannel(false);*/

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
            Game.Events.PublicLog += e => Info.Channel.SendMessageAsync(e.Message);
            Game.Events.Finished += OnFinished;
            Game.Events.PlayerSwitched += OnPlayerSwitched;
            Game.Events.PlayersSwappedHands += e => Task.WaitAll(SendHandTo(e.Player), SendHandTo(e.Target));

            Game.Events.WentOut += e => SendToChannelAsync(
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

        private async void OnFinished()
        {
            var sb = new StringBuilder();
            if (Players.Any(p => p.State == PlayerState.Out || p.IsActive()))
            {
                sb.AppendLine("The game has finished with the following results:");
                GetScoreboardLines().ForEach(l => sb.AppendLine(l));
            }

            sb.Append($"`The game lasted {GetTimeString()}`");

            await SendToChannelAsync(sb.ToString());
            Finished?.Invoke();
        }

        private string GetTimeString()
        {
            if (startTime == null)
                return "a few moments";
            var time = DateTime.UtcNow - startTime;
            return time > TimeSpan.FromDays(1) ? "more than a day" : $@"{time:hh\:mm\:ss\.fff}";
        }

        public Task SendToChannelAsync(string message) => Info.Channel.SendMessageAsync(message);

        private async void OnPlayerReceivedCards(PlayerReceivedCardsEvent e)
        {
            if (e.Player is DiscordUserPlayer dPlayer && e.Cards.Any())
                await dPlayer.User.SendCardsAsync(e.Cards, CardDisplayStyle, true);
        }

        private void OnPlayerSwitched(PlayerSwitchedEvent @event)
        {
            selectedCenterRowCard = null;
            Task.WaitAll(SendHandTo(@event.NextPlayer),
                         SendTableToChannel(false, $"Now it's {@event.NextPlayer}'s turn"));
        }

        public Task SendTableToChannel(bool addPlayersStats, string title = null) =>
            SendTableToChannel(addPlayersStats, CardDisplayStyle, title);

        private async Task SendTableToChannel(bool addPlayersStats, CardDisplayStyle displayStyle, string title)
        {
            if (!IsGameStarted)
            {
                await Info.Channel.SendMessageAsync("Game has not started yet");
            }
            else
            {
                var eb = new EmbedBuilder
                {
                    Title = title
                };

                if (addPlayersStats)
                {
                    var messageBuilder = new StringBuilder();
                    foreach (var player in Game.Players.Where(p => p.IsActive()))
                        messageBuilder.Append($"{player} - {player.Hand.Count.Pluralize("card", "cards")}\n");
                    eb.AddField(fb => fb.WithName("Players:").WithValue(messageBuilder.ToString()));
                }

                DecorateEmbed(eb, displayStyle);
                await Info.Channel.SendMessageAsync(embed: eb.Build());
            }
        }

        private async Task SendHandTo(Player player)
        {
            if (!(player is DiscordUserPlayer dPlayer))
                return;
            try
            {
                await dPlayer.User.SendCardsAsync(dPlayer.Hand, CardDisplayStyle);
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
                var turnIndex = Game.CurrentTurnIndex;

                var result = selectedCenterRowCard == null ||
                             args.Contains(" on ", StringComparison.InvariantCultureIgnoreCase)
                    ? CardParser.ParseMatchCards(args)
                                .IfSuccess(r => Game.MatchCenterRowCard(player,
                                                                        r.Value.target,
                                                                        r.Value.matchers))
                    : CardParser.ParseCards(args)
                                .IfSuccess(r => Game.MatchCenterRowCard(
                                               player, selectedCenterRowCard.Value,
                                               r.Value.ToArray()))
                                .DoIfSuccess(r => { selectedCenterRowCard = null; });

                return await ProcessResultMessage(result, turnIndex);
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
                await SendTableToChannel(
                    false, $"Selected **{card}**. Now use `dos match <card(s)>` to match it with something");
                return Result.Success();
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

                var turnIndex = Game.CurrentTurnIndex;
                
                return await ProcessResultMessage(
                    Game.AddCardToCenterRow(player, matchResult.Value.First()), turnIndex);
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
                var turnIndex = Game.CurrentTurnIndex;
                return await ProcessResultMessage(Game.EndTurn(player), turnIndex);
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

        public Result SwitchToHybridCardDisplayStyle(IUser user)
        {
            if (UseHybridCardDisplayStyle)
                return Result.Fail("Already showing both card names and images");

            if (Owner.Id != user.Id)
                return Result.Fail(
                    $"Only owner of this game (**{Owner?.Username}**) can switch to hybrid display style");

            UseHybridCardDisplayStyle = true;
            return Result.Success("Now printing names of cards with their images");
        }

        public void DecorateEmbed(EmbedBuilder builder, CardDisplayStyle displayStyle)
        {
            if (selectedCenterRowCard != null)
            {
                builder.AddField(fb => fb.WithName("Selected card:").WithValue(selectedCenterRowCard.Value));
                if (displayStyle.IsImage())
                    builder.ThumbnailUrl = selectedCenterRowCard.Value.ToImageUrl();
            }

            const string iconUrl =
                "https://cdn.discordapp.com/app-icons/660931324642590720/9ddff8a1683b190b04e846130ce997c9.png";

            builder.WithAuthor($"{Info.ServerName} #{Info.Channel.Name} — Turn №{Game.CurrentTurnIndex + 1}", iconUrl);

            var leader = Game.LeaderByCardCount;
            builder
               .WithFields(
                    new EmbedFieldBuilder()
                       .WithName("Current player:")
                       .WithValue(Game.CurrentPlayer.Name)
                       .WithIsInline(true),
                    new EmbedFieldBuilder()
                       .WithName("Next player:")
                       .WithValue(Game.NextPlayer.Name)
                       .WithIsInline(true))
               .WithFooter(fb => fb.WithText($"Game time - {GetTimeString()} | Cards dealt: {Game.CardsDealtCount}"));

            if (displayStyle.IsImage())
                builder.WithCardsImage(
                    Game.CenterRow.Zip(Game.CenterRowAdditional, (c, cl) => cl.Prepend(c).ToList()).ToList(),
                    false);

            if (displayStyle.IsText())
                builder.AddField(fb => fb.WithName("Center row").WithValue(string.Join("\n", Game.GameTableLines())));

            builder.WithCurrentTimestamp();
        }

        private async Task<Result> ProcessResultMessage(Result result, int currentTurnBeforeAction)
        {
            if (result.IsFail || !result.HasMessage || currentTurnBeforeAction != Game.CurrentTurnIndex)
                return result;

            if(!IsFinished)
                await SendTableToChannel(false, result.Message);
            return Result.Success();
        }
    }
}
