using System.Collections.Generic;

namespace TCG.Core
{
    public abstract class GameBase
    {
        public GameState GameState { get; protected set; }

        protected PhaseBase currentPhase; // TODO: フェーズ管理もGameStateに移管予定

        protected GameBase()
        {
            GameState = new GameState(this);
        }

        public virtual void StartGame(GameState state)
        {
            SetupGame(state);
        }

        protected abstract void SetupGame(GameState state);

        public virtual void NextTurn(GameState state)
        {
            state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.Players.Count;
        }

        public virtual void NextPhase(GameState state)
        {
            if (currentPhase != null)
            {
                // TODO: フェーズ遷移のロジックをGameStateと連携する形に修正
                // currentPhase = currentPhase.GetNextPhase(); 
            }
        }
    }
}
