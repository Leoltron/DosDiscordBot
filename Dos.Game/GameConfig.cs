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
            DoubleColorMatchDraw = 1;
        }

        public ushort CalloutPenalty { get; set; }
        public ushort FalseCalloutPenalty { get; set; }
        public ushort InitialHandSize { get; set; }
        public ushort MinCenterRowSize { get; set; }
        public ushort DoubleColorMatchDraw { get; set; }
        public bool CenterRowPenalty { get; set; }
        public bool DrawEndsTurn { get; set; }
        public bool SevenSwap { get; set; }
        public uint? StartingPlayer { get; set; }
        public bool CardCountRanking { get;  set; } = false;
    }
}
