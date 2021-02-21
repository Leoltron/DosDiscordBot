using Dos.Game;

namespace Dos.Database.Models
{
    public class BotGameConfig : GameConfig
    {
        public ushort Decks { get; set; } = 1;
        public bool UseImages { get; set; } = true;
        public bool AllowGameStop { get; set; } = true;
        public bool SaveReplays { get; set; }
        public bool PublishReplays { get; set; }

        public string ToDiscordTable() =>
            string.Join("\n",
                        "```cs",
                        $"Decks                {Decks}",
                        $"CalloutPenalty       {CalloutPenalty}",
                        $"FalseCalloutPenalty  {FalseCalloutPenalty}",
                        $"InitialHandSize      {InitialHandSize}",
                        $"MinCenterRowSize     {MinCenterRowSize}",
                        $"DoubleColorMatchDraw {DoubleColorMatchDraw}",
                        "",
                        $"CenterRowPenalty     {CenterRowPenalty.ToString().ToLower()}",
                        $"DrawEndsTurn         {DrawEndsTurn.ToString().ToLower()}",
                        $"SevenSwap            {SevenSwap.ToString().ToLower()}",
                        "",
                        $"UseImages            {UseImages.ToString().ToLower()}",
                        $"AllowGameStop        {AllowGameStop.ToString().ToLower()}",
                        $"CardCountRanking     {CardCountRanking.ToString().ToLower()}",
                        "",
                        $"SaveReplays          {SaveReplays.ToString().ToLower()}",
                        $"PublishReplays       {PublishReplays.ToString().ToLower()}",
                        "```");
    }
}
