using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Parses ability cost strings and creates a list of ICost objects.
    /// </summary>
    public static class CostFactory
    {
        // Defines a mapping from a regex pattern to a function that creates an ICost object.
        private static readonly Dictionary<Regex, System.Func<Match, ICost>> CostParsers = new()
        {
            // Matches costs like (1), (2), etc.
            { new Regex(@"\((\d+)\)"), match => new StockCost<WeissCard>(int.Parse(match.Groups[1].Value)) },
            // Matches costs like (C1), (C2), etc.
            { new Regex(@"\(C(\d+)\)"), match => new ClockCost<WeissCard>(int.Parse(match.Groups[1].Value)) },
            // Matches a specific discard cost text
            { new Regex(@"手札のキャラを(\d+)枚控え室に置く"), match => new DiscardCost(int.Parse(match.Groups[1].Value), card => (card.Data as WeissCardData)?.CardType == "Character") },
            // Matches resting the source card itself
            { new Regex(@"このカードを【レスト】する"), match => new RestCost(isSourceCard: true) }
        };

        /// <summary>
        /// Parses a cost string and returns a list of corresponding ICost objects.
        /// </summary>
        /// <param name="costString">The string representing the combined cost, e.g., "(1) 手札のキャラを１枚控え室に置く".</param>
        /// <returns>A list of ICost objects found in the string.</returns>
        public static List<ICost> ParseCosts(string costString)
        {
            var costs = new List<ICost>();
            if (string.IsNullOrWhiteSpace(costString)) return costs;

            string remainingCostString = costString;

            // Loop until all parts of the cost string are parsed
            while (!string.IsNullOrWhiteSpace(remainingCostString))
            {
                bool matchFound = false;
                foreach (var pair in CostParsers)
                {
                    var regex = pair.Key;
                    var match = regex.Match(remainingCostString);
                    if (match.Success)
                    {
                        costs.Add(pair.Value(match));
                        // Remove the matched part from the string to process the rest
                        remainingCostString = remainingCostString.Remove(match.Index, match.Length).Trim();
                        matchFound = true;
                        break; // Restart the loop to ensure correct order of parsing
                    }
                }

                // If no known cost pattern was found in the remainder of the string, stop parsing.
                if (!matchFound)
                {
                    // You might want to log a warning here for unparsed cost parts
                    // UnityEngine.Debug.LogWarning($"Unrecognized cost part: {remainingCostString}");
                    break;
                }
            }

            return costs;
        }
    }
}
