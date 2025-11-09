using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// Effect to return a character from the waiting room to hand.
    /// </summary>
    public class ReturnFromWaitingRoomEffect : IEffect
    {
        public int Count { get; } // Number of cards to return
        // Potentially add a filter for card type (e.g., "キャラ" - character)

        public ReturnFromWaitingRoomEffect(int count)
        {
            Count = count;
        }

        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // This is a simplified implementation.
            // A full implementation would require player interaction to choose the card(s).
            UnityEngine.Debug.Log($"Effect: Return {Count} card(s) from waiting room to hand. (Not fully implemented)");
        }
    }
}
