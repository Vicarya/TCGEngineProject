using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 【注意】このクラスは 'DrawCost' という名前でICostを実装していますが、その実態は「山札からN枚カードを引く」という効果（Effect）です。
    /// また、このクラスは `DeckCost.cs` と完全に同一の実装であり、重複しています。
    /// 設計上、IEffectインターフェースを実装する、あるいは `DeckCost.cs` との統合を検討するべきです。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class DrawCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">山札から手札に加えるカードの枚数。</param>
        public DrawCost(int amount)
        {
            this.amount = amount;
        }

        /// <summary>
        /// 山札に指定された枚数のカードが存在するかを確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">プレイヤー。</param>
        /// <param name="source">発生源のカード。</param>
        /// <returns>山札に十分なカードがあればtrue、なければfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var deck = player.GetZone<IDeckZone<TCard>>();
            return deck != null && deck.Cards.Count >= amount;
        }

        /// <summary>
        /// 実際に山札からカードを手札に加えます。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">プレイヤー。</param>
        /// <param name="source">発生源のカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var deck = player.GetZone<IDeckZone<TCard>>();
            var hand = player.GetZone<IHandZone<TCard>>();

            if (deck == null || hand == null) return;

            for (int i = 0; i < amount; i++)
            {
                if (deck.Cards.Count == 0) break;
                var card = deck.Cards.First();
                deck.RemoveCard(card);
                hand.AddCard(card);
            }
        }

        /// <summary>
        /// UI表示用の説明文を返します。
        /// </summary>
        /// <returns>処理内容を説明する文字列。</returns>
        public string GetDescription()
        {
            return $"カードを{amount}枚引く";
        }
    }
}
