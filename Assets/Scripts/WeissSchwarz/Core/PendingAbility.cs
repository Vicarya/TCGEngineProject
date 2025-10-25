
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Represents an ability that has been triggered but is waiting to be resolved.
    /// </summary>
    public class PendingAbility
    {
        /// <summary>
        /// The card that the ability belongs to.
        /// </summary>
        public WeissCard SourceCard { get; }

        /// <summary>
        /// The raw text of the ability.
        /// </summary>
        public string AbilityText { get; }

        /// <summary>
        /// The player who gets to resolve this ability.
        /// </summary>
        public WeissPlayer ResolvingPlayer { get; }

        public PendingAbility(WeissCard sourceCard, string abilityText, WeissPlayer resolvingPlayer)
        {
            SourceCard = sourceCard;
            AbilityText = abilityText;
            ResolvingPlayer = resolvingPlayer;
        }

        public override string ToString()
        {
            return $"[{SourceCard.Data.CardCode}] - {AbilityText}";
        }
    }
}
