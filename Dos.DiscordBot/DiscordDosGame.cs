using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.DiscordBot.Util;
using Dos.Game;
using Dos.Game.Deck;
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
        private readonly ILogger logger;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public IUser Owner;
        public List<Player> Players = new List<Player>();

        private Card? selectedCenterRowCard;

        public DiscordDosGame(ISocketMessageChannel channel, IUser owner, ILogger logger, string serverName)
        {
            this.logger = logger;
            Info = new DiscordDosGameInfo(serverName, CreateDate, channel, owner);
            AddUserPlayer(owner);
            Owner = owner;

            LogInfo($"{owner.Username} have created the game");
        }

        public DiscordDosGameInfo Info { get; }

        public DateTime CreateDate { get; } = DateTime.UtcNow;
        public Dictionary<ulong, DiscordUserPlayer> IdToUserPlayers { get; } = new Dictionary<ulong, DiscordUserPlayer>();
        public DosGame Game { get; private set; }
        public bool IsGameStarted => Game != null;

        public bool IsFinished => IsGameStarted && Game.CurrentState.IsFinished;

        public BotGameConfig Config { get; } = new BotGameConfig();

        private void AddUserPlayer(IUser user)
        {
            var player = new DiscordUserPlayer(Players.Count, user);
            Players.Add(player);
            IdToUserPlayers[user.Id] = player;
        }

        public event Action Finished;

        private void LogInfo(string message)
        {
            logger.Information($"[{Info.ServerName} - #{Info.Channel.Name}] [game] {message}");
        }

        public async Task<Result> JoinAsync(IUser user)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (IdToUserPlayers.ContainsKey(user.Id))
                    return Result.Fail("You have already joined this game");

                AddUserPlayer(user);

                return Result.Success($"{user.Username} have joined the game!");
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

                Game = new DosGame(new ShufflingDealer(Decks.Classic.Times(Config.Decks).ToArray()),
                                   Players.ToArray(),
                                   Config);
                Game.Finished += Finished;

                await Task.WhenAll(Players.Select(SendHandTo));
                await SendTableToChannel(false);
                Game.PlayerSwitched += OnPlayerSwitched;
                Game.PlayerReceivedCards += OnPlayerReceivedCards;

                Game.CalledOut += (caller, target) =>
                    LogInfo($"{caller} called out {target}");
                Game.DosCall += caller => LogInfo($"{caller} called DOS!");
                Game.FalseCallout += caller => LogInfo($"{caller} made a false callout");
                Game.PlayerAddedCard += (p, card) => LogInfo($"{p} added {card} to the Center Row");
                Game.PlayerMatchedCard += (p, cards, target) =>
                    LogInfo($"{p} put {string.Join(" and ", cards)} to {target}");

                LogInfo($"Game started, Center Row: {string.Join(", ", Game.CenterRow)}");
                Players.ForEach(p => LogInfo($"{p}'s hand: {p.Hand.ToLogString()}"));

                return Result.Success();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private void OnPlayerReceivedCards(Player player, Card[] cards)
        {
            LogInfo($"{player.Name} received [{cards.ToLogString()}]");

            if (player is DiscordUserPlayer dPlayer && cards.Any())
                dPlayer.User.SendCardsAsync(cards, Config.UseImages, true).Wait();
        }

        private async void OnPlayerSwitched(Player nextPlayer, int unmatchedCardsCount)
        {
            selectedCenterRowCard = null;
            await SendHandTo(nextPlayer);
            LogInfo($"{nextPlayer.Name}'s turn, hand: {Game.CurrentPlayerHand.ToLogString()}");
            if (Config.CenterRowPenalty && unmatchedCardsCount > 0)
                await Info.Channel.SendMessageAsync(
                    $"There are {unmatchedCardsCount} unmatched card(s). Draw the same amount.");

            await SendTableToChannel(false);
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
                    foreach (var (player, cardsCount) in Game.HandsTable)
                        messageBuilder.Append($"{player} - {cardsCount} {(cardsCount == 1 ? "card" : "cards")}\n");

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

                await Info.Channel.SendMessageAsync($"Now it's **{Game.CurrentPlayerName}**'s turn");
            }
        }

        private async Task SendHandTo(Player playerIndex)
        {
            if (!(playerIndex is DiscordUserPlayer dPlayer))
                return;
            try
            {
                await dPlayer.User.SendCardsAsync(dPlayer.Hand, Config.UseImages);
            }
            catch (Exception e)
            {
                logger.Warning(e, $"Failed to send hand to @{dPlayer.Name}");
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
    }
}
