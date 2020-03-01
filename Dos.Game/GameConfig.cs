namespace Dos.Game
{
    public class GameConfig
    {
        public GameConfig()
        {
            MinCenterRowSize = 2;
            InitialHandSize = 7;
            FalseCalloutPenalty = 2;
            CalloutPenalty = 2;
            StartingPlayer = null;
        }

        public ushort CalloutPenalty { get; set; }
        public ushort FalseCalloutPenalty { get; set; }
        public ushort InitialHandSize { get; set; }
        public ushort MinCenterRowSize { get; set; }
        public bool CenterRowPenalty { get; set; }
        public bool DrawEndsTurn { get; set; }
        public bool SevenSwap { get; set; }
        public uint? StartingPlayer { get; set; }
    }
}
