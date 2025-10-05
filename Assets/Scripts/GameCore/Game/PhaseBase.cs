using System;
using System.Collections.Generic;
namespace TCG.Core {
    public abstract class PhaseBase {
        public string Id { get; }
        public string Name { get; }
        public PhaseBase Parent { get; private set; }
        private List<PhaseBase> subPhases = new();
        public IReadOnlyList<PhaseBase> SubPhases => subPhases.AsReadOnly();

        public PhaseBase(string id, string name) { Id = id; Name = name; }
        public void AddSubPhase(PhaseBase p) { p.Parent = this; subPhases.Add(p); }

        // フェーズを実行する（同期的にシンプルに）
        public virtual void OnEnter(TCG.Core.GameState state) {
            state.EventBus.Raise(new GameEvent(BaseGameEvents.PhaseStarted, this));
        }
        public virtual void OnExit(TCG.Core.GameState state) {
            state.EventBus.Raise(new GameEvent(BaseGameEvents.PhaseEnded, this));
        }

        // 実行: デフォルトは入れ子順でEnter->children->Exit
        public virtual void Execute(TCG.Core.GameState state) {
            OnEnter(state);
            foreach (var sp in subPhases) sp.Execute(state);
            OnExit(state);
        }
    }
}
