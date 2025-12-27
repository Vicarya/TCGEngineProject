using TCG.Core;
using TCG.Weiss;

namespace TCG.Weiss
{
    /// <summary>
    /// 特定のカードインスタンス自身をレスト状態にするコストを表現します。
    /// 【注意】RestCost(isSourceCard: true) と機能的に非常に似ていますが、実装のアプローチが異なります。
    /// こちらは特定のカードインスタンスに強く結合するのに対し、RestCostはより汎用的な作りになっています。
    /// 将来的にはどちらかに統一することが望ましいかもしれません。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class RestSelfCost<TCard> : ICost where TCard : Card
    {
        private readonly TCard card;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="card">レストする対象のカードインスタンス。</param>
        public RestSelfCost(TCard card)
        {
            this.card = card;
        }

        /// <summary>
        /// 対象のカードがコストを支払える状態にあるか（＝スタンド状態か）を確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源カード。指定されていればコンストラクタ引数より優先されます。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var target = source as TCard ?? card;
            // すでにレストしているなら支払不可
            return target != null && !target.IsRested;
        }

        /// <summary>
        /// 実際にカードをレストしてコストを支払います。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源カード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var target = source as TCard ?? card;
            if (target != null) target.SetRested(true);
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            return "このカードを【レスト】する";
        }
    }
}
