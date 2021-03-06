using System;
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
        public string Name { get; }
        public bool IsAi { get; }

        public IList<Card> Hand { get; set; } = new List<Card>();

        public int HandScore => Hand.Sum(c => c.Points);

        public PlayerState State { get; set; } = PlayerState.WaitingForGameToStart;
        public int? ScoreBoardPosition;
        public bool CanBeCalledOut { get; set; } = false;

        public override string ToString() => Name;

        public virtual void Play(DosGame game) => throw new InvalidOperationException();
    }
}
