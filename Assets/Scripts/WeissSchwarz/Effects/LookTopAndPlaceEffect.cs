using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// 「山札の上からN枚を見て、好きな順番で山札の上か下に置く」という効果を処理することを意図したクラス。
    /// TCG.Core.IEffectインターフェースを実装しています。
    /// 【重要】現在、実際の効果処理（プレイヤーによる選択など）は実装されておらず、未完成です。
    /// </summary>
    public class LookTopAndPlaceEffect : IEffect
    {
        /// <summary>
        /// 山札の上から見るカードの枚数。
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="count">山札の上から見るカードの枚数。</param>
        public LookTopAndPlaceEffect(int count)
        {
            Count = count;
        }

        /// <summary>
        /// 山札のトップカードを操作する効果を解決します。
        /// 【未実装】現在、このメソッドの中身は実装されていません。
        /// </summary>
        /// <param name="gameEvent">この効果をトリガーしたゲームイベント。</param>
        /// <param name="gameState">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // NOTE: これは簡略化された実装です。
            //       完全な実装では、プレイヤーにどのカードをどの順番で山札の上/下に置くかを選択させるインタラクションが必要です。
            UnityEngine.Debug.Log($"効果: 山札の上から{Count}枚を見て、上か下に置く。(未完全実装)");
        }
    }
}
