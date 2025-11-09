using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Represents the cost of resting one or more cards.
    /// </summary>
    public class RestCost : ICost
    {
        /// <summary>
        /// If true, this cost requires resting the card that owns the ability.
        /// </summary>
        public bool IsSourceCard { get; }

        public RestCost(bool isSourceCard = false)
        {
            IsSourceCard = isSourceCard;
        }

        public bool CanPay(GameState state, Player player, Card source)
        {
            if (IsSourceCard)
            {
                // The source card itself must be able to rest.
                return source != null && !source.IsRested;
            }
            // TODO: Implement logic for selecting other cards to rest.
            return false;
        }

        public void Pay(GameState state, Player player, Card source)
        {
            if (IsSourceCard && source != null)
            {
                // Use the base Card API to mark as rested. If the concrete card type has extra behavior
                // (e.g., WeissCard.Rest sets IsTapped), replicate that behavior here for compatibility.
                source.SetRested(true);
                source.IsTapped = true;
            }
        }

        public string GetDescription()
        {
            if (IsSourceCard)
            {
                return "このカードを【レスト】する";
            }
            return "指定されたカードを【レスト】する";
        }
    }
}
