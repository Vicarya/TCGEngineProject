using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;

namespace TCG.Weiss.Effects
{
    public static class EffectFactory
    {
        // Defines a mapping from a regex pattern to a function that creates an IEffect object.
        private static readonly Dictionary<Regex, System.Func<Match, IEffect>> EffectParsers = new()
        {
            // Matches "パワーを＋X" (Power +X)
            { new Regex(@"パワーを＋(\d+)"), match => new PowerBoostEffect(int.Parse(match.Groups[1].Value)) },
            // Matches "ソウルを＋X" (Soul +X)
            { new Regex(@"ソウルを＋(\d+)"), match => new SoulBoostEffect(int.Parse(match.Groups[1].Value)) },
            // Add more effect parsers here in the future
        };

        /// <summary>
        /// Parses an effect description string and returns a list of corresponding IEffect objects.
        /// </summary>
        /// <param name="description">The string representing the ability's effects.</param>
        /// <returns>A list of IEffect objects found in the string.</returns>
        public static List<IEffect> ParseEffects(string description)
        {
            var effects = new List<IEffect>();
            if (string.IsNullOrWhiteSpace(description)) return effects;

            // For now, we assume one effect per description string for simplicity.
            // A more complex implementation could loop through the string to find multiple effects.
            foreach (var pair in EffectParsers)
            {
                var regex = pair.Key;
                var match = regex.Match(description);
                if (match.Success)
                {
                    effects.Add(pair.Value(match));
                    // For now, we stop after the first match.
                    break;
                }
            }

            if (effects.Count == 0)
            {
                // UnityEngine.Debug.LogWarning($"No parsable effect found in description: '{description}'");
            }

            return effects;
        }
    }
}
