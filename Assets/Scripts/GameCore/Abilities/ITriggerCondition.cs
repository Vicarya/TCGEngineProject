namespace TCG.Core
{
    /// <summary>
    /// 自動能力などが発動するための「きっかけ」となる条件を抽象化するインターフェース。
    /// 例：「このカードが舞台に置かれた時」「アタックした時」など。
    /// </summary>
    public interface ITriggerCondition
    {
        /// <summary>
        /// 指定されたゲームイベントや状態が、このトリガー条件を満たすかどうかを判定します。
        /// </summary>
        /// <param name="e">発生したゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="source">このトリガーを持つ能力の発生源であるカード。</param>
        /// <returns>条件を満たしていればtrue、そうでなければfalse。</returns>
        bool IsSatisfied(GameEvent e, GameState state, Card source);
    }
}
