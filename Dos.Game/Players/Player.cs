using System.Collections.Generic;
using System.Linq;
using Dos.Game.Model;

namespace Dos.Game.Players
{
    public class Player
    {
        public Player(int orderId, string name, bool isAi)
        {
            OrderId = orderId;
            Name = name;
            IsAi = isAi;
        }

        public int OrderId { get; set; }
        public virtual string Name { get; }
        public bool IsAi { get; }

        public IList<Card> Hand { get; } = new List<Card>();

        public int HandScore => Hand.Sum(c => c.Points);

        public PlayerState State { get; set; } = PlayerState.WaitingForGameToStart;
        public bool CanBeCalledOut { get; set; } = false;
    }
}
