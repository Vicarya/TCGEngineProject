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
                UnityEngine.Debug.Log($"Applying +{Amount} soul to {source.Data.Name}. (Note: This is a temporary effect and not fully implemented).");
            }
        }
    }
}
