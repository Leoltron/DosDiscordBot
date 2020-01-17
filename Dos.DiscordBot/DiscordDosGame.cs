using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.DiscordBot.Util;
using Dos.Game.Deck;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using Dos.Utils;
using Serilog;

namespace Dos.DiscordBot
{
    public class DiscordDosGame
    {
        private readonly ILogger logger;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public DiscordDosGameInfo Info { get; }

        private Card? selectedCenterRowCard;

        public DiscordDosGame(ISocketMessageChannel channel, IUser owner, ILogger logger, string serverName)
        {
            this.logger = logger;
            Info = new DiscordDosGameInfo(serverName, CreateDate, channel, owner);
            PlayerIds = new List<ulong> {owner.Id};
            Players = new Dictionary<ulong, IUser> {[owner.Id] = owner};

            LogInfo($"{owner.Username} have created the game");
        }

        public DateTime CreateDate { get; } = DateTime.UtcNow;

        public IUser Owner => PlayerIds.Any() ? Players[PlayerIds[0]] : null;
        public Dictionary<ulong, IUser> Players { get; }
        private List<ulong> PlayerIds { get; }
        public Game.Game Game { get; private set; }
        public bool IsGameStarted => Game != null;

        public event Action Finished;

        public bool IsFinished => IsGameStarted && Game.CurrentState.IsFinished;

        public GameConfig Config { get; } = new GameConfig();

        private ulong CurrentPlayerId => PlayerIds[Game.CurrentPlayer];

        private IUser CurrentUser => User(Game.CurrentPlayer);

        private void LogInfo(string message)
        {
            logger.Information($"[{Info.ServerName} - #{Info.Channel.Name}] [game] {message}");
        }

        public async Task<Result> JoinAsync(IUser player)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (PlayerIds.Contains(player.Id)) return Result.Fail("You have already joined this game");

                PlayerIds.Add(player.Id);
                Players.Add(player.Id, player);

                return Result.Success($"{player.Username} have joined the game!");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> StartAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            if (IsGameStarted) return Result.Fail("Game has already started");

            await semaphoreSlim.WaitAsync();
            try
            {
                if (Owner.Id != player.Id)
                    return Result.Fail($"Only owner of this game (**{Owner?.Username}**) can start it");

                Game = new Game.Game(new ShuffledDeckGenerator(Decks.Classic), Players.Count, 7)
                {
                    PlayerNames = PlayerIds.Select((id, i) => (i, Players[id].Username)).ToDictionary(),
                    CalloutPenalty = 2,
                    FalseCalloutPenalty = 2
                };
                Game.Finished += Finished;

                await Task.WhenAll(Enumerable.Range(0, Players.Count).Select(SendHandTo));
                await SendTableToChannel(false);
                Game.PlayerSwitch += OnPlayerSwitch;
                Game.PlayerReceivedCards +=
                    (id, cards) =>
                    {
                        LogInfo($"{User(id).DiscordTag()} received [{cards.ToLogString()}]");
                        Players[PlayerIds[id]].SendCards(cards, Config.UseImages, true);
                    };

                Game.CalledOut += (caller, calledOut) =>
                    LogInfo($"{User(caller).DiscordTag()} called out {User(calledOut).DiscordTag()}");
                Game.DosCall += caller => LogInfo($"{User(caller).DiscordTag()} called DOS!");
                Game.FalseCallout += caller => LogInfo($"{User(caller).DiscordTag()} made a false callout");
                Game.PlayerAddedCard += (i, card) => LogInfo($"{User(i).DiscordTag()} added {card} to the Center Row");
                Game.PlayerMatchedCard += (i, cards, target) =>
                    LogInfo($"{User(i).DiscordTag()} put {string.Join(" and ", cards)} to {target}");

                LogInfo($"Game started, Center Row: {string.Join(", ", Game.centerRow)}");
                for (var i = 0; i < Players.Count; i++)
                    LogInfo($"{User(i).DiscordTag()}'s hand: {Game.playerHands[i].ToLogString()}");

                return Result.Success();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private void OnPlayerSwitch(int nextPlayer)
        {
            selectedCenterRowCard = null;
            SendHandTo(nextPlayer);
            LogInfo($"{CurrentUser.DiscordTag()}'s turn, hand: {Game.CurrentPlayerHand.ToLogString()}");
            SendTableToChannel(false);
        }

        private IUser User(int i) => Players[PlayerIds[i]];

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
                    for (var i = 0; i < PlayerIds.Count; i++)
                    {
                        var handSize = Game.playerHands[i].Count;
                        messageBuilder.Append(
                            $"{Game.GetPlayerName(i)} - {handSize} {(handSize == 1 ? "card" : "cards")}\n");
                    }

                    await Info.Channel.SendMessageAsync(messageBuilder.ToString());
                }

                await Info.Channel.SendMessageAsync("**Center Row:**");
                if (useImages)
                    await Info.Channel.SendCards(
                        Game.centerRow.Select((c, i) => Game.centerRowAdditional[i].Prepend(c).ToList()));
                else
                    await Info.Channel.SendMessageAsync(string.Join("\n", Game.GameTableLines()));

                if (selectedCenterRowCard != null)
                    await Info.Channel.SendMessageAsync($"Selected **{selectedCenterRowCard.Value}**");

                await Info.Channel.SendMessageAsync($"Now it's **{Game.CurrentPlayerName}**'s turn");
            }
        }

        private Task<IUserMessage> SendHandTo(int playerIndex)
        {
            var id = PlayerIds[playerIndex];
            return Players[id].SendCards(Game.playerHands[playerIndex], Config.UseImages);
        }

        public async Task<Result> MatchAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                var playerIndex = PlayerIds.IndexOf(player.Id);
                if (selectedCenterRowCard == null || args.Contains(" on ", StringComparison.InvariantCultureIgnoreCase))
                    return CardParser.ParseMatchCards(args)
                                     .IfSuccess(result => Game.MatchCenterRowCard(playerIndex,
                                                                                  result.Value.target,
                                                                                  result.Value.matchers));
                else
                    return CardParser.ParseCards(args)
                                     .IfSuccess(r => Game.MatchCenterRowCard(
                                                    playerIndex, selectedCenterRowCard.Value,
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

        public async Task<Result> SelectAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                if (CurrentPlayerId != player.Id) return Result.Fail("It's not your turn right now.");

                if (Game.CurrentState is AddingToCenterRowState)
                    return Result.Fail("You cannot match cards if you already started adding cards to the Center Row");

                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;

                if (matchResult.Value.IsEmpty())
                    return Result.Fail("If you want to select card, you need to tell me which one");

                var card = matchResult.Value.First();
                if (!Game.centerRow.Contains(card)) return Result.Fail($"There's no **{card}** in the Center Row");

                selectedCenterRowCard = card;
                return Result.Success($"Selected **{card}**. Now use `dos match <card(s)>` to match it with something");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> AddToCenterRowAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;
                else if (matchResult.Value.IsEmpty())
                    return Result.Fail("You have to put something there");
                else
                    return Game.AddCardToCenterRow(PlayerIds.IndexOf(player.Id), matchResult.Value.First());
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SendHandAsync(IUser player, bool reportToChannel)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                await SendHandTo(PlayerIds.IndexOf(player.Id));
                return Result.Success(reportToChannel ? "DM'd you your hand" : null);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> EndTurnAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.EndTurn(PlayerIds.IndexOf(player.Id));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> DrawAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.Draw(PlayerIds.IndexOf(player.Id));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> CallDosAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.CallDos(PlayerIds.IndexOf(player.Id));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> CalloutAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id)) return Result.Fail();

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.Callout(PlayerIds.IndexOf(player.Id));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
