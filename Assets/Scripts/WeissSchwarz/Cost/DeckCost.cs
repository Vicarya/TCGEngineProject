using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 【注意】このクラスは 'DeckCost' という名前ですが、現在の実装は「山札からN枚カードを引く」という効果（Effect）になっています。
    /// 本来、コストはリソースを支払う行為ですが、このクラスはリソース（手札）を増やす動作をします。
    /// 設計上、IEffectインターフェースを実装する方が適切である可能性があります。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class DeckCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">山札から手札に加えるカードの枚数。</param>
        public DeckCost(int amount)
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
            var deckZone = player.GetZone<IDeckZone<TCard>>();
            return deckZone != null && deckZone.Cards.Count >= amount;
        }

        /// <summary>
        /// 実際に山札からカードを手札に加えます。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">プレイヤー。</param>
        /// <param name="source">発生源のカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var deckZone = player.GetZone<IDeckZone<TCard>>();
            var handZone = player.GetZone<IHandZone<TCard>>();

            if (deckZone == null || handZone == null) return;

            for (int i = 0; i < amount; i++)
            {
                if (deckZone.Cards.Count == 0) break;
                var card = deckZone.Cards.First(); // 山札の一番上から
                deckZone.RemoveCard(card);
                handZone.AddCard(card);
            }
        }

        /// <summary>
        /// UI表示用の説明文を返します。
        /// </summary>
        /// <returns>処理内容を説明する文字列。</returns>
        public string GetDescription()
        {
            return $"山札から{amount}枚引く";
        }
    }
}