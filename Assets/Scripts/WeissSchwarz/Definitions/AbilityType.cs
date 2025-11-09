namespace TCG.Weiss
{
    /// <summary>
    /// Defines the type of a card ability.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>
        /// An automatic ability that triggers on a specific event. (【自】)
        /// </summary>
        Auto,

        /// <summary>
        /// An activated ability that requires a cost to be paid. (【起】)
        /// </summary>
        Activated,

        /// <summary>
        /// A continuous ability that is always active. (【永】)
        /// </summary>
        Continuous
    }
}
