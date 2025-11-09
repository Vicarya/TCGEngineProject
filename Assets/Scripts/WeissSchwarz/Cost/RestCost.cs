using System.Threading.Tasks;
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

        public bool CanPay(Player player, Card source)
        {
            if (IsSourceCard)
            {
                // The source card itself must be able to rest.
                return source != null && !source.IsResting;
            }
            // TODO: Implement logic for selecting other cards to rest.
            return false;
        }

        public async Task Pay(Player player, Card source)
        {
            if (IsSourceCard && source != null)
            {
                source.Rest();
            }
            // The await is here to satisfy the async requirement of the interface.
            await Task.CompletedTask;
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
