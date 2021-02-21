using System;
using System.Collections.Generic;
using Dos.Database.Models;
using Dos.Utils;

namespace Dos.DiscordBot.Util
{
    public static class BotGameConfigExtensions
    {
        public static Result Set(this BotGameConfig config, string key, string value)
        {
            var action = Setters.GetValueOrDefault(key);
            return action == null
                ? Result.Fail($"Sorry, I don't know configuration key \"{key}\".")
                : action(config, value);
        }

        public static string GetDescription(string config) =>
            Descriptions.GetValueOrDefault(config, $"Sorry, I don't know configuration \"{config}\".");

        private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Decks"] = "Amount of decks used in game",
            ["CalloutPenalty"] = "Penalty for being called out for not saying DOS while having two cards",
            ["FalseCalloutPenalty"] = "Penalty for false callout",
            ["InitialHandSize"] = "Amount of cards given to the players at the start of the game",
            ["MinCenterRowSize"] = "Minimal amount of cards on the center row.",
            ["DoubleColorMatchDraw"] = "Amount of cards other players draw when you make a Double Color Match.",
            ["CenterRowPenalty"] =
                "Enables **Center Row Penalty*** house rule: at the end of your turn, draw as much cards as you left unmatched in the center row",
            ["DrawEndsTurn"] =
                "Enables **Draw Ends Turn** house rule: if you draw a card, your turn ends immediately",
            ["SevenSwap"] =
                "Enables **Seven Swap** house rule: Color Match on 7 will force you to switch your hand with somebody else",
            ["UseImages"] = "If enabled, images will be used instead of text to show cards",
            ["AllowGameStop"] = "Allows game owner to use `dos stop` to forcefully end the game",
            ["CardCountRanking"] =
                "If enabled, card count will be used to rank players in stopped game instead of card score",
            ["SaveReplays"] = "Allows to save games for rewatching them after",
            ["PublishReplays"] = "Makes replays public",
        };

        private static readonly Dictionary<string, Func<BotGameConfig, string, Result>> Setters =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Decks"] = (c, s) => TryParseUShort(s, 1, 100).DoIfSuccess(v => c.Decks = v.Value),
                ["CalloutPenalty"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.CalloutPenalty = v.Value),
                ["FalseCalloutPenalty"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.FalseCalloutPenalty = v.Value),
                ["InitialHandSize"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.InitialHandSize = v.Value),
                ["MinCenterRowSize"] =
                    (c, s) => TryParseUShort(s, 1, 10).DoIfSuccess(v => c.MinCenterRowSize = v.Value),
                ["DoubleColorMatchDraw"] =
                    (c, s) => TryParseUShort(s).DoIfSuccess(v => c.DoubleColorMatchDraw = v.Value),
                ["CenterRowPenalty"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.CenterRowPenalty = v.Value),
                ["DrawEndsTurn"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.DrawEndsTurn = v.Value),
                ["SevenSwap"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.SevenSwap = v.Value),
                ["AllRules"] = (c, s) => TryParseBool(s).DoIfSuccess(v =>
                {
                    c.CenterRowPenalty = v.Value;
                    c.DrawEndsTurn = v.Value;
                    c.SevenSwap = v.Value;
                }),
                ["UseImages"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.UseImages = v.Value),
                ["AllowGameStop"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.AllowGameStop = v.Value),
                ["CardCountRanking"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.CardCountRanking = v.Value),
                ["SaveReplays"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.SaveReplays = v.Value),
                ["PublishReplays"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.PublishReplays = v.Value),
            };

        private static Result<ushort> TryParseUShort(string s,
                                                     ushort min = ushort.MinValue,
                                                     ushort max = ushort.MaxValue)
        {
            if (!ushort.TryParse(s, out var value))
                return ushort.MinValue.ToFail($"Failed to parse {s}");

            if (value < min)
                return value.ToFail($"Value must be greater or equal to {min}");

            if (value > max)
                return value.ToFail($"Value must be less or equal to {max}");

            return value.ToSuccess();
        }

        private static Result<bool> TryParseBool(string s) =>
            bool.TryParse(s, out var value) ? value.ToSuccess() : false.ToFail($"Failed to parse {s}");
    }
}
