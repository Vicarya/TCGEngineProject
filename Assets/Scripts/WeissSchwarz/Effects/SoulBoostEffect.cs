using TCG.Core;

namespace TCG.Weiss.Effects
{
    public class SoulBoostEffect : IEffect
    {
        public int Amount { get; }

        public SoulBoostEffect(int amount)
        {
            Amount = amount;
        }

        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // Similar to PowerBoostEffect, this is a simplified implementation.
            // A full implementation would need targeting and duration tracking.
            if (source != null)
            {
                // Source may be a CardBase<WeissCardData> or a concrete WeissCard. Cast safely.
                if (source is TCG.Core.CardBase<TCG.Weiss.WeissCardData> cardWithData)
                {
                    UnityEngine.Debug.Log($"Applying +{Amount} soul to {cardWithData.Data.Name}. (Note: This is a temporary effect and not fully implemented).");
                }
                else
                {
                    UnityEngine.Debug.Log($"Applying +{Amount} soul to an unknown card (no Data). (Note: This is a temporary effect and not fully implemented).");
                }
            }
        }
    }
}
