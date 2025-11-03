using System.Text.RegularExpressions;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Parses ability cost strings and creates ICost objects.
    /// </summary>
    public static class CostFactory
    {
        // Matches costs like (1), (2), etc.
        private static readonly Regex StockCostRegex = new Regex(@"^\((\d+)\)$");

        /// <summary>
        /// Parses a cost string and returns the corresponding ICost object.
        /// </summary>
        /// <param name="costString">The string representing the cost, e.g., "(1)".</param>
        /// <returns>An ICost object, or null if the cost string is not recognized.</returns>
        public static ICost Parse(string costString)
        {
            if (string.IsNullOrWhiteSpace(costString)) return null;

            // Try to parse as a stock cost
            var stockMatch = StockCostRegex.Match(costString);
            if (stockMatch.Success)
            {
                if (int.TryParse(stockMatch.Groups[1].Value, out int amount))
                {
                    return new StockCost<WeissCard>(amount);
                }
            }

            if (costString == "手札のキャラを１枚控え室に置く")
            {
                return new DiscardCost(1, card => (card.Data as WeissCardData)?.CardType == "Character");
            }

            // TODO: Add parsing for other cost types

            return null;
        }
    }
}
