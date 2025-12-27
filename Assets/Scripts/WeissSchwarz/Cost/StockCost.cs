using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 「ストックをN枚支払う」というコストを表現・処理するクラス。
    /// TCG.Core.ICostインターフェースを実装しています。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class StockCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        /// <summary>
        /// 新しいストックコストのインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">支払うストックの枚数。</param>
        public StockCost(int amount) => this.amount = amount;

        /// <summary>
        /// プレイヤーがコストを支払える状態にあるかを確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var stock = player.GetZone<IStockZone<TCard>>();
            return stock != null && stock.Cards.Count >= amount;
        }

        /// <summary>
        /// 実際にコストを支払います。ストックから指定枚数のカードを控え室に移動します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var stock = player.GetZone<IStockZone<TCard>>();
            var discardPile = player.GetZone<IDiscardPile<TCard>>();

            if (stock == null || discardPile == null)
            {
                // 本来はエラーログや例外を発生させるべき
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                if (stock.Cards.Count == 0) break;
                // ストックの一番上からカードを取得
                var card = stock.Cards.First(); 
                stock.RemoveCard(card);
                discardPile.AddCard(card);
            }
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            return $"ストックを{amount}枚払う";
        }
    }
}
