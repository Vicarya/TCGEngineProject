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
            // Matches "あなたは自分の山札を上からX枚見て、山札の上か下に置く"
            { new Regex(@"あなたは自分の山札を上から(\d+)枚見て、山札の上か下に置く"), match => new LookTopAndPlaceEffect(int.Parse(match.Groups[1].Value)) },
            // Matches "あなたは自分の控え室のキャラをX枚選び、手札に戻す"
            { new Regex(@"あなたは自分の控え室のキャラを(\d+)枚選び、手札に戻す"), match => new ReturnFromWaitingRoomEffect(int.Parse(match.Groups[1].Value)) },
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

            // Iterate through all parsers and find all matching effects in the description.
            // This allows for multiple effects in a single description string.
            foreach (var pair in EffectParsers)
            {
                var regex = pair.Key;
                foreach (Match match in regex.Matches(description))
                {
                    effects.Add(pair.Value(match));
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
