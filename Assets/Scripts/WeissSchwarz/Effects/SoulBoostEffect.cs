using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// 「カードのソウルを一時的に増減させる」効果を処理することを意図したクラス。
    /// TCG.Core.IEffectインターフェースを実装しています。
    /// 【重要】現在、実際のソウル修正処理はゲーム状態に反映されておらず、未完成です。
    /// また、一時的な効果の管理（ターン終了時のリセットなど）も実装されていません。
    /// PowerBoostEffectと同様に、対象の指定や期間の追跡といったロジックも必要です。
    /// </summary>
    public class SoulBoostEffect : IEffect
    {
        /// <summary>
        /// ソウルの修正量（増減値）。
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">ソウルの修正量。</param>
        public SoulBoostEffect(int amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// カードのソウルを修正する効果を解決します。
        /// 【未実装】現在、このメソッドの中身は実装されていません。
        /// </summary>
        /// <param name="gameEvent">この効果をトリガーしたゲームイベント。</param>
        /// <param name="gameState">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // NOTE: PowerBoostEffectと同様に、これは簡略化された実装です。
            //       実際のソウル修正処理はゲーム状態に反映されていません。
            //       適切な実装には、対象の指定と、一時的な効果を追跡し、
            //       ターン終了時にそれが期限切れになるような管理システムが必要になります。
            if (source != null)
            {
                if (source is TCG.Core.CardBase<TCG.Weiss.WeissCardData> cardWithData)
                {
                    // 例えば cardWithData.CurrentSoul += Amount; (CurrentSoulプロパティは存在しない) のような処理が必要
                    UnityEngine.Debug.Log($"効果: {cardWithData.Data.Name} のソウルを +{Amount} 修正。（未完全実装）");
                }
                else
                {
                    UnityEngine.Debug.Log($"効果: 不明なカードのソウルを +{Amount} 修正。（未完全実装）");
                }
            }
        }
    }
}
