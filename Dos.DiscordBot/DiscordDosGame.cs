using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
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
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public ISocketMessageChannel Channel { get; }
        public IUser Owner => PlayerIds.Any() ? Players[PlayerIds[0]] : null;
        public Dictionary<ulong, IUser> Players { get; }
        private List<ulong> PlayerIds { get; }
        public Game.Game Game { get; private set; }
        public bool IsGameStarted => Game != null;

        private Card? selectedCenterRowCard;

        private readonly string serverName;
        private readonly ILogger logger;

        public bool IsFinished => IsGameStarted && Game.CurrentState.IsFinished;

        public DiscordDosGame(ISocketMessageChannel channel, IUser owner, ILogger logger, string serverName)
        {
            Channel = channel;
            this.logger = logger;
            this.serverName = serverName;
            PlayerIds = new List<ulong> {owner.Id};
            Players = new Dictionary<ulong, IUser> {[owner.Id] = owner};

            Info($"{owner.Username} have created the game");
        }

        private void Info(string message)
        {
            logger.Information($"[{serverName} - #{Channel.Name}] {message}");
        }

        public async Task<Result> JoinAsync(IUser player)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (PlayerIds.Contains(player.Id))
                {
                    return Result.Fail("You have already joined this game");
                }

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
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            if (IsGameStarted)
            {
                return Result.Fail("Game has already started");
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (Owner.Id != player.Id)
                {
                    return Result.Fail($"Only owner of this game (**{Owner?.Username}**) can start it");
                }

                Game = new Game.Game(new ShuffledDeckGenerator(Decks.Classic), Players.Count, 7)
                {
                    PlayerNames = PlayerIds.Select((id, i) => (i, Players[id].Username)).ToDictionary()
                };
                await Task.WhenAll(Enumerable.Range(0, Players.Count).Select(SendHandTo));
                await SendShortTable();
                Game.PlayerSwitch += OnPlayerSwitch;
                Game.PlayerReceivedCards += (id, cards) =>
                    Players[PlayerIds[id]].SendMessageAsync("You were dealt " + cards.ToDiscordString());

                Info($"Game started, center row: {string.Join(", ", Game.centerRow)}");
                for (int i = 0; i < Players.Count; i++)
                {
                    Info($"{Game.GetPlayerName(i)}'s hand: {Game.playerHands[i].ToDiscordString()}");
                }

                return Result.Success($"\u200b\nPlay! (It's your turn, **{Game.CurrentPlayerName}**)");
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
            Channel.SendMessageAsync(
                $"{GetTable(false).Message}\n\u200b\nNow it's your turn, **{Game.CurrentPlayerName}**");
        }

        public Task<RestUserMessage> SendFullTable() => Channel.SendMessageAsync(GetTable(true).Message);

        public Task<RestUserMessage> SendShortTable() => Channel.SendMessageAsync(GetTable(false).Message);

        public Result GetTable(bool addPlayersStats)
        {
            if (!IsGameStarted)
            {
                return Result.Fail("Game has not started yet");
            }

            var lines = new List<string>();

            if (addPlayersStats)
                for (var i = 0; i < PlayerIds.Count; i++)
                {
                    var handSize = Game.playerHands[i].Count;
                    lines.Add($"{Game.GetPlayerName(i)} - {handSize} {(handSize == 1 ? "card" : "cards")}");
                }

            lines.AddRange(Game.GameTableLines());

            return Result.Success(string.Join("\n", lines));
        }

        private Task<IUserMessage> SendHandTo(int playerIndex)
        {
            var id = PlayerIds[playerIndex];
            return Players[id].SendMessageAsync(string.Join("\n", Game.GetPlayerHandLines(playerIndex)));
        }

        private async Task<Result> MatchAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (selectedCenterRowCard == null || args.Contains(" on ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var matchResult = CardParser.ParseMatchCards(args);
                    return matchResult.IsFail
                        ? matchResult
                        : Game.MatchCenterRowCard(PlayerIds.IndexOf(player.Id), matchResult.Value.target,
                                                  matchResult.Value.matchers);
                }
                else
                {
                    return CardParser.ParseCards(args)
                                     .IfSuccess(r => Game.MatchCenterRowCard(
                                                    PlayerIds.IndexOf(player.Id), selectedCenterRowCard.Value,
                                                    CardParser.ParseCards(args).Value.ToArray()))
                                     .IfSuccess(r =>
                                      {
                                          selectedCenterRowCard = null;
                                          return r;
                                      });
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> SelectAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (CurrentPlayerId != player.Id)
                {
                    return Result.Fail("It's not your turn right now.");
                }

                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;

                if (matchResult.Value.IsEmpty())
                {
                    return Result.Fail("If you want to select card, you need to tell me which one");
                }

                var card = matchResult.Value.First();
                if (!Game.centerRow.Contains(card))
                {
                    return Result.Fail($"There's no **{card}** in the Center Row");
                }

                selectedCenterRowCard = card;
                return Result.Success($"Selected **{card}**. Now use `dos play <card(s)>` to match it with something");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private ulong CurrentPlayerId => PlayerIds[Game.CurrentPlayer];

        public Task<Result> PlayAsync(IUser player, string args) =>
            Game.CurrentState is MatchingCenterRowState
                ? MatchAsync(player, args)
                : AddToCenterRowAsync(player, args);

        private async Task<Result> AddToCenterRowAsync(IUser player, string args)
        {
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                var matchResult = CardParser.ParseCards(args);
                if (matchResult.IsFail)
                    return matchResult;
                else if (matchResult.Value.IsEmpty())
                {
                    return Result.Fail("You have to put something there");
                }
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
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                await player.SendMessageAsync(
                    string.Join("\n", Game.GetPlayerHandLines(PlayerIds.IndexOf(player.Id))));
                return Result.Success(reportToChannel ? "DM'd you your hand" : null);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result> FinishMatchingAsync(IUser player)
        {
            if (!Players.ContainsKey(player.Id))
            {
                return Result.Fail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                return Game.FinishMatching(PlayerIds.IndexOf(player.Id));
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
