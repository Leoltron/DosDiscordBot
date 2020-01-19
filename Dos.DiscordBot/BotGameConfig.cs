using System;
using System.Collections.Generic;
using Dos.Game;
using Dos.Utils;

namespace Dos.DiscordBot
{
    public class BotGameConfig : GameConfig
    {
        private static readonly Dictionary<string, string> Descriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Decks"] = "Decks description",
                ["CalloutPenalty"] = "CalloutPenalty description",
                ["FalseCalloutPenalty"] = "FalseCalloutPenalty description",
                ["InitialHandSize"] = "InitialHandSize description",
                ["MinCenterRowSize"] = "MinCenterRowSize description",
                ["CenterRowPenalty"] = "CenterRowPenalty description",
                ["UseImages"] = "UseImages description"
            };

        private static readonly Dictionary<string, Func<BotGameConfig, string, Result>> Setters =
            new Dictionary<string, Func<BotGameConfig, string, Result>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Decks"] = (c, s) => TryParseUShort(s, 1, 100).DoIfSuccess(v => c.Decks = v.Value),
                ["CalloutPenalty"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.CalloutPenalty = v.Value),
                ["FalseCalloutPenalty"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.FalseCalloutPenalty = v.Value),
                ["InitialHandSize"] = (c, s) => TryParseUShort(s).DoIfSuccess(v => c.InitialHandSize = v.Value),
                ["MinCenterRowSize"] = (c, s) => TryParseUShort(s, 1, 10).DoIfSuccess(v => c.MinCenterRowSize = v.Value),
                ["CenterRowPenalty"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.CenterRowPenalty = v.Value),
                ["UseImages"] = (c, s) => TryParseBool(s).DoIfSuccess(v => c.UseImages = v.Value)
            };

        public ushort Decks { get; private set; } = 1;
        public bool UseImages { get; private set; } = true;

        public string ToDiscordTable() =>
            "```cs\n" +
            $"Decks                {Decks}\n" +
            $"CalloutPenalty       {CalloutPenalty}\n" +
            $"FalseCalloutPenalty  {FalseCalloutPenalty}\n" +
            $"InitialHandSize      {InitialHandSize}\n" +
            $"MinCenterRowSize     {MinCenterRowSize}\n" +
            "\n" +
            $"CenterRowPenalty     {CenterRowPenalty.ToString().ToLower()}\n" +
            "\n" +
            $"UseImages            {UseImages.ToString().ToLower()}\n" +
            "```";

        public static string GetDescription(string config) =>
            Descriptions.GetValueOrDefault(config, $"Sorry, I don't know configuration \"{config}\".");

        public Result Set(string key, string value)
        {
            var action = Setters.GetValueOrDefault(key);
            return action == null
                ? Result.Fail($"Sorry, I don't know configuration key \"{key}\".")
                : action(this, value);
        }

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
