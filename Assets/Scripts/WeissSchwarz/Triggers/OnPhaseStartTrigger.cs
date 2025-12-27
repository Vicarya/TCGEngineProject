using TCG.Core;

namespace TCG.Weiss.Triggers
{
    /// <summary>
    /// 特定の、または任意の「フェイズ開始時」に能力が誘発するための条件をチェックするクラス。
    /// TCG.Core.ITriggerConditionインターフェースを実装しています。
    /// </summary>
    public class OnPhaseStartTrigger : ITriggerCondition
    {
        // このトリガーが反応するフェイズのID。nullまたは空の場合、任意のフェイズ開始に反応する。
        private readonly string _phaseId;

        /// <summary>
        /// 新しいOnPhaseStartTriggerインスタンスを初期化します。
        /// </summary>
        /// <param name="phaseId">このトリガーが反応するフェイズのID。指定しない場合は全てのフェイズ開始に反応。</param>
        public OnPhaseStartTrigger(string phaseId = null)
        {
            _phaseId = phaseId;
        }

        /// <summary>
        /// 発生したゲームイベントがこのトリガーの条件（＝特定のフェイズが開始されたか）を満たしているかを判定します。
        /// </summary>
        /// <param name="e">判定対象のゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="source">トリガーの発生源であるカード（未使用）。</param>
        /// <returns>条件を満たしている場合はtrue、そうでない場合はfalse。</returns>
        public bool IsSatisfied(GameEvent e, GameState state, Card source)
        {
            // 1. イベントが「フェイズ開始イベント」であるかチェック
            if (e.Type != BaseGameEvents.PhaseStarted) return false;

            // 2. イベントデータがPhaseBaseのインスタンスであるかチェック
            if (e.Data is not PhaseBase phase) return false;

            // 3. _phaseIdが指定されていなければ常にtrue（任意のフェイズ開始に反応）
            //    指定されていれば、イベントデータに含まれるフェイズのIDと一致するかチェック
            return string.IsNullOrEmpty(_phaseId) || phase.Id == _phaseId;
        }
    }
}
