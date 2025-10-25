
using TCG.Core;

namespace TCG.Weiss
{
    public class WeissGameState : GameState
    {
        public AbilityQueue AbilityQueue { get; }

        public WeissGameState(GameBase game) : base(game)
        {
            AbilityQueue = new AbilityQueue();
        }
    }
}
