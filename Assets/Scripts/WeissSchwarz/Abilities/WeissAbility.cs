using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Represents a Weiss Schwarz specific ability, extending the core AbilityBase
    /// with game-specific properties like AbilityType.
    /// </summary>
    public class WeissAbility : AbilityBase
    {
        /// <summary>
        /// The type of the ability (Auto, Activated, or Continuous).
        /// </summary>
        public AbilityType AbilityType { get; set; }

        /// <summary>
        /// The raw text description of the ability's trigger and effects.
        /// </summary>
        public string Description { get; set; }

        public WeissAbility(Card source) : base(source)
        {
        }
    }
}
