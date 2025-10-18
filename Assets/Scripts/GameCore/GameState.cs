using System.Collections.Generic;

namespace TCG.Core {
    public class GameState {
        public GameBase Game { get; }
        public EventBus EventBus { get; }
        public string CurrentPhaseId { get; set; }
        public int CurrentPlayerIndex { get; set; } = 0;
        public int TurnCounter { get; set; } = 0;
        public Player ActivePlayer => Players.Count > CurrentPlayerIndex ? Players[CurrentPlayerIndex] : null;
        public List<Player> Players { get; } = new();

        public GameState(GameBase game)
        {
            Game = game;
            EventBus = new EventBus();
        }
    }
}
