using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// 「カードのパワーを一時的に増減させる」効果を処理することを意図したクラス。
    /// TCG.Core.IEffectインターフェースを実装しています。
    /// 【重要】現在、実際のパワー修正処理はゲーム状態に反映されておらず、未完成です。
    /// また、一時的な効果の管理（ターン終了時のリセットなど）も実装されていません。
    /// </summary>
    public class PowerBoostEffect : IEffect
    {
        /// <summary>
        /// パワーの修正量（増減値）。
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">パワーの修正量。</param>
        public PowerBoostEffect(int amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// カードのパワーを修正する効果を解決します。
        /// 【未実装】現在、このメソッドの中身は実装されていません。
        /// </summary>
        /// <param name="gameEvent">この効果をトリガーしたゲームイベント。</param>
        /// <param name="gameState">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        public void Resolve(GameEvent gameEvent, GameState gameState, Card source)
        {
            // NOTE: 現状は効果の発生源カード自身を対象と仮定しているが、
            //       より複雑な対象選択ロジックが必要になる。
            if (source != null)
            {
                // NOTE: これは一時的なパワー修正ですが、実際のゲーム状態には反映されていません。
                //       適切な実装には、一時的な効果を追跡し、ターン終了時にそれが期限切れになるような
                //       管理システム（例: EffectManager）が必要になります。
                if (source is TCG.Core.CardBase<TCG.Weiss.WeissCardData> cardWithData)
                {
                    // 例えば source.TemporaryPower += Amount; のような処理が必要
                    UnityEngine.Debug.Log($"効果: {cardWithData.Data.Name} のパワーを +{Amount} 修正。（未完全実装）");
                }
                else
                {
                    UnityEngine.Debug.Log($"効果: 不明なカードのパワーを +{Amount} 修正。（未完全実装）");
                }
            }
        }
    }
}
