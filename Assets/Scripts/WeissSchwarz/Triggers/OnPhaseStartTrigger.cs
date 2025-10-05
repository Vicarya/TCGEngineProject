using TCG.Core;

namespace TCG.Weiss.Triggers
{
    public class OnPhaseStartTrigger : ITriggerCondition
    {
        private readonly string _phaseId;

        public OnPhaseStartTrigger(string phaseId = null)
        {
            _phaseId = phaseId;
        }

        public bool IsSatisfied(GameEvent e, GameState state, Card source)
        {
            // イベントがフェーズ開始イベントかチェック
            if (e.Type != BaseGameEvents.PhaseStarted) return false;

            // イベントデータがPhaseBaseのインスタンスかチェック
            if (e.Data is not PhaseBase phase) return false;

            // フェーズIDが指定されていなければ常にtrue、指定されていればIDが一致するかチェック
            return string.IsNullOrEmpty(_phaseId) || phase.Id == _phaseId;
        }
    }
}
