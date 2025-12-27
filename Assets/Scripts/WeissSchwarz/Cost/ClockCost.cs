using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 「手札のカードをN枚クロック置場に置く」というコストを表現・処理するクラス。
    /// TCG.Core.ICostインターフェースを実装しています。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class ClockCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        /// <summary>
        /// 新しいクロックコストのインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">手札からクロック置場に置くカードの枚数。</param>
        public ClockCost(int amount)
        {
            this.amount = amount;
        }

        /// <summary>
        /// プレイヤーがコストを支払える状態にあるか（＝手札に十分なカードがあるか）を確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            return hand != null && hand.Cards.Count >= amount;
        }

        /// <summary>
        /// 実際にコストを支払います。手札から指定枚数のカードをクロック置場に移動します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            var clockZone = player.GetZone<IClockZone<TCard>>();

            if (hand == null || clockZone == null) return;

            // TODO: どのカードを支払うかプレイヤーに選択させるべき。現状は手札の最初のカードから自動で選択している。
            for (int i = 0; i < amount; i++)
            {
                if (hand.Cards.Count == 0) break;
                var card = hand.Cards.First();
                hand.RemoveCard(card);
                clockZone.AddCard(card);
            }
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            // NOTE: この説明は「クロックにあるカードを払う」とも解釈できるため、やや不正確。
            // 実装は「手札からクロックに置く」コスト。
            return $"クロックを{amount}枚払う";
        }
    }
}
