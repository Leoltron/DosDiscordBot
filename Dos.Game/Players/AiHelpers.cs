using System.Collections.Generic;
using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Match;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.Players
{
    internal static class AiHelpers
    {
        // Taken from www.fantasynamegenerators.com
        private static readonly string[] BotNames = {
            "Achiever", "Admin", "Alive", "Alpha", "Analyst", "Angel", "Anima", "Animus", "Answer", "Apex", "Aspect",
            "Assist", "Aura", "Aurora", "Aware", "Base", "Beauty", "Bit", "Blossom", "Brain", "Butler", "Care", "Carer",
            "Center", "Central", "Cerebrum", "Cerebrus", "Cloud", "Code", "Codec", "Codex", "Colossus", "Companion",
            "Cosmic", "Cosmos", "Creator", "Cube", "Data", "Deus", "Different", "Dimension", "Discovery", "Dock",
            "Dream", "Echo", "Ego", "Energy", "Enigma", "Expert", "Face", "Familiar", "Father", "Feature", "Feel",
            "Figure", "Fluke", "Flux", "Form", "Frame", "Friend", "Fruit", "Gabriel", "Genesis", "Ghost", "Gift",
            "Golem", "Guard", "Guardian", "Guest", "Guide", "Harmony", "Heart", "Helix", "Hello", "Hex", "Holmes",
            "Hope", "Idea", "Image", "Impulse", "Intra", "Junior", "Life", "Light", "Logic", "Luck", "Lucky", "Lumos",
            "Machina", "Made", "Mage", "Master", "Matrix", "Max", "Memory", "Mind", "Mindful", "Mother", "Nemo", "Neo",
            "Nerve", "Omega", "Omni", "One", "Optix", "Oracle", "Original", "Patch", "Phoenix", "Pixel", "Present",
            "Prime", "Prism", "Reply", "Response", "Saint", "Sample", "Science", "Search", "Self", "Shell", "Shift",
            "Signal", "Solace", "Sole", "Soul", "Spark", "Spirit", "Sprite", "Stranger", "Student", "Switch", "Synapse",
            "Synergy", "System", "Tec", "Tech", "Test", "Thing", "Thinkerer", "Thought", "Tweak", "Unique", "Unit",
            "User", "Vessel", "Ware", "Watcher", "Whole", "Wish", "Witness", "Wonder", "Zen", "Zero"
        };

        public static string GenerateBotName(int order) => $"{BotNames.RandomElement()} AI";

        public static IEnumerable<(Card[] matchers, Card target)> GetAvailableMatches(
            IList<Card> possibleMatchers, IList<Card> possibleTargets)
        {
            foreach (var target in possibleTargets.Distinct())
                for (var i = 0; i < possibleMatchers.Count; i++)
                {
                    var first = possibleMatchers[i];

                    if (target.Matches(first))
                        yield return (new[] {first}, target);

                    for (var j = i+1; j < possibleMatchers.Count; j++)
                    {
                        var matchers = new[] {first, possibleMatchers[j]};
                        if (target.Matches(matchers))
                            yield return (matchers, target);
                    }
                }
        }

        public static double MatcherWeight(this Card card)
        {
            if (card.Value == CardValue.Sharp)
                return 0.5;

            if (card.Color == CardColor.Wild)
            {
                return 1;
            }

            return (double) card.Value;
        }

        public static double CardToAddWeight(this Card card)
        {
            if (card.Value == CardValue.Sharp)
            {
                return 0;
            }

            return (double) card.Value;
        }

        public static double MatchWeightMultiplier(this MatchType matchType) =>
            matchType switch
            {
                MatchType.NoMatch => 0,
                MatchType.SingleMatch => 1,
                MatchType.DoubleMatch => 1.5,
                MatchType.SingleColorMatch => 1.7,
                MatchType.DoubleColorMatch => 2,
                _ => 1
            };
    }
}
