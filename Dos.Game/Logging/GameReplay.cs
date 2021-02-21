using System;

namespace Dos.Game.Logging
{
    public class GameReplay
    {
        public string[] PlayerNames { get; }
        public GameLogEvent[] Events { get; }
        public GameSnapshot[] Snapshots { get; }
        public DateTime GameDateUtc { get; }

        public GameReplay(string[] playerNames, GameLogEvent[] events, GameSnapshot[] snapshots, DateTime gameDateUtc)
        {
            PlayerNames = playerNames;
            Events = events;
            Snapshots = snapshots;
            GameDateUtc = gameDateUtc;
        }
    }
}
