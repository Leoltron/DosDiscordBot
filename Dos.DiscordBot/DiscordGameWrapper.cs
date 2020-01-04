using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Dos.Game.Deck;
using Dos.Game.Deck.Generation;
using Dos.Game.Extensions;
using Dos.Game.Util;

namespace Dos.DiscordBot
{
    public class DiscordGameWrapper
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public ISocketMessageChannel Channel { get; }
        public IUser Owner => PlayerIds.Any() ? Players[PlayerIds[0]] : null;
        public Dictionary<ulong, IUser> Players { get; }
        private List<ulong> PlayerIds { get; }
        public Game.Game Game { get; private set; }
        public bool IsGameStarted => Game != null;

        public DiscordGameWrapper(ISocketMessageChannel channel, IUser owner)
        {
            Channel = channel;
            PlayerIds = new List<ulong> {owner.Id};
            Players = new Dictionary<ulong, IUser> {[owner.Id] = owner};
        }

        public async Task<Result<string>> JoinAsync(IUser player)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (PlayerIds.Contains(player.Id))
                {
                    return "You have already joined this game".ToFail();
                }

                PlayerIds.Add(player.Id);
                Players.Add(player.Id, player);

                return $"{player.Username} have joined the game!".ToSuccess();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Result<string>> StartAsync(IUser player)
        {
            if (IsGameStarted)
            {
                return "Game has already started".ToFail();
            }

            await semaphoreSlim.WaitAsync();
            try
            {
                if (Owner.Id != player.Id)
                {
                    return $"Only owner of this game (**{Owner?.Username}**) can start it".ToFail();
                }

                Game = new Game.Game(new ShuffledDeckGenerator(Decks.Classic), Players.Count, 7)
                {
                    PlayerNames = PlayerIds.Select((id, i) => (i, Players[id].Username)).ToDictionary()
                };
                return $"Play! (It's your turn, {Game.CurrentPlayerName})".ToSuccess();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
