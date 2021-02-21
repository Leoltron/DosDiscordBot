using System.Collections.Generic;
using Dos.Game.Model;

namespace Dos.Game.Logging
{
    public class GameSnapshot
    {
        public int? CurrentPlayerId { get; }
        public Dictionary<int, Card[]> PlayerHands { get; }
        public Card[][] CenterRow { get; }

        public GameSnapshot(int? currentPlayerId, Dictionary<int, Card[]> playerHands, Card[][] centerRow)
        {
            CurrentPlayerId = currentPlayerId;
            PlayerHands = playerHands;
            CenterRow = centerRow;
        }
    }
}
