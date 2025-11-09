using TCG.Core;

namespace TCG.Weiss.Effects
{
    public class PowerBoostEffect : IEffect
    {
        public int Amount { get; }

        public PowerBoostEffect(int amount)
        {
            Amount = amount;
        }

        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // For now, let's assume the effect targets the source card itself.
            // More complex targeting logic will be needed later.
            if (source != null)
            {
                // This is a temporary power boost. A proper implementation would need
                // a way to track temporary effects and have them expire at the end of the turn.
                // For now, we'll just log it.
                UnityEngine.Debug.Log($"Applying +{Amount} power to {source.Data.Name}. (Note: This is a temporary effect and not fully implemented).");
            }
        }
    }
}
