using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// 「控え室のカードをN枚選び、手札に戻す」効果を処理することを意図したクラス。
    /// TCG.Core.IEffectインターフェースを実装しています。
    /// 【重要】現在、実際の効果処理（プレイヤーによるカード選択など）は実装されておらず、未完成です。
    /// </summary>
    public class ReturnFromWaitingRoomEffect : IEffect
    {
        /// <summary>
        /// 控え室から手札に戻すカードの枚数。
        /// </summary>
        public int Count { get; } 
        
        // TODO: 将来的に、カードタイプ（例: 「キャラ」）などのフィルタリング機能を追加する必要がある。

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="count">控え室から手札に戻すカードの枚数。</param>
        public ReturnFromWaitingRoomEffect(int count)
        {
            Count = count;
        }

        /// <summary>
        /// 控え室からカードを手札に戻す効果を解決します。
        /// 【未実装】現在、このメソッドの中身は実装されていません。
        /// </summary>
        /// <param name="gameEvent">この効果をトリガーしたゲームイベント。</param>
        /// <param name="gameState">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // NOTE: これは簡略化された実装です。
            //       完全な実装では、プレイヤーにどのカードを回収するかを選択させるインタラクションが必要です。
            //       選択されたカードを控え室から手札に移動するロジックも必要になります。
            UnityEngine.Debug.Log($"効果: 控え室から{Count}枚のカードを手札に戻す。(未完全実装)");
        }
    }
}
