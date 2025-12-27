using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 「舞台のカードをN枚控え室に置く」というコストを表現・処理するクラス。
    /// TCG.Core.ICostインターフェースを実装しています。
    /// </summary>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public class StageToWaitingRoomCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">舞台から控え室に置くカードの枚数。</param>
        public StageToWaitingRoomCost(int amount)
        {
            this.amount = amount;
        }

        /// <summary>
        /// プレイヤーがコストを支払える状態にあるか（＝舞台に十分なカードがあるか）を確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var stageZone = player.GetZone<IStageZone<TCard>>();
            return stageZone != null && stageZone.Cards.Count >= amount;
        }

        /// <summary>
        /// 実際にコストを支払います。舞台のカードを指定枚数だけ控え室に移動します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            var stageZone = player.GetZone<IStageZone<TCard>>();
            var discardPile = player.GetZone<IDiscardPile<TCard>>();

            if (stageZone == null || discardPile == null) return;

            // TODO: 実際のゲームでは、どのカードをコストとして支払うかプレイヤーに選択させる必要がある。
            //       現状は簡略化のため、舞台の先頭のカードから自動的に選択している。
            //       DiscardCostのように、IWeissPlayerControllerを介して選択させる実装が望ましい。
            for (int i = 0; i < amount; i++)
            {
                if (stageZone.Cards.Count == 0) break;
                var card = stageZone.Cards.First(); 
                stageZone.RemoveCard(card);
                discardPile.AddCard(card);
            }
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            return $"舞台のカードを{amount}枚控え室に置く";
        }
    }
}
