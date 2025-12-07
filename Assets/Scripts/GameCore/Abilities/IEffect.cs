namespace TCG.Core
{
    /// <summary>
    /// カードの能力が解決されたときに実行される実際の「効果」を抽象化するインターフェース。
    /// 例：「カードを1枚引く」「相手に1ダメージを与える」など。
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// 効果を解決（実行）します。
        /// </summary>
        /// <param name="e">この効果の解決をトリガーしたゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        void Resolve(GameEvent e, GameState state, Card source);
    }
}