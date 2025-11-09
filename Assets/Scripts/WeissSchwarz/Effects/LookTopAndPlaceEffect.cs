using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// Effect to look at the top card of the deck and place it on top or bottom.
    /// </summary>
    public class LookTopAndPlaceEffect : IEffect
    {
        public int Count { get; } // Number of cards to look at

        public LookTopAndPlaceEffect(int count)
        {
            Count = count;
        }

        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // This is a simplified implementation.
            // A full implementation would require player interaction to choose top/bottom.
            UnityEngine.Debug.Log($"Effect: Look at top {Count} card(s) of deck and place on top/bottom. (Not fully implemented)");
        }
    }
}
