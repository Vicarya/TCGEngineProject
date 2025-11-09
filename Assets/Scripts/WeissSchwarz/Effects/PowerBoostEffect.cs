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
                // For now, we'll attempt to log target card's name if Data is available.
                if (source is TCG.Core.CardBase<TCG.Weiss.WeissCardData> cardWithData)
                {
                    UnityEngine.Debug.Log($"Applying +{Amount} power to {cardWithData.Data.Name}. (Note: This is a temporary effect and not fully implemented).");
                }
                else
                {
                    UnityEngine.Debug.Log($"Applying +{Amount} power to an unknown card (no Data). (Note: This is a temporary effect and not fully implemented).");
                }
            }
        }
    }
}
