using System;

namespace TCG.Core {
    public class ActivationWindow {
        public string[] AllowedPhaseIds = new string[0]; // phase id 文字列マッチ
        public string[] AllowedEventTypes = new string[0]; // EventBus の Type
        public Func<GameState, GameEvent, bool> CustomPredicate; // 任意判定

        public bool IsActive(GameState state, GameEvent ev = null) {
            if (CustomPredicate != null && CustomPredicate(state, ev)) return true;
            if (ev != null && Array.Exists(AllowedEventTypes, s => s == ev.Type.Name)) return true;
            if (state!=null && Array.Exists(AllowedPhaseIds, s => s == state.CurrentPhaseId)) return true;
            return false;
        }
    }
}
