using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 「特定の条件を満たす手札のカードをN枚捨てる」というコストを表現・処理するクラス。
    /// TCG.Core.ICostインターフェースを実装しています。
    /// </summary>
    public class DiscardCost : ICost
    {
        // 捨てる枚数
        private readonly int _amount;
        // 捨てることができるカードの条件を指定する述語（例：カードタイプが「キャラクター」であること）
        private readonly Predicate<WeissCard> _filter;

        /// <summary>
        /// 新しい手札破棄コストのインスタンスを初期化します。
        /// </summary>
        /// <param name="amount">捨てるカードの枚数。</param>
        /// <param name="filter">捨てることができるカードの条件。nullの場合は任意のカードが対象となる。</param>
        public DiscardCost(int amount, Predicate<WeissCard> filter)
        {
            _amount = amount;
            _filter = filter ?? (card => true); // filterが指定されなければ、全てのカードを有効とみなす
        }

        /// <summary>
        /// プレイヤーがコストを支払える状態にあるか（＝条件を満たすカードが手札に十分な枚数あるか）を確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            var hand = player.GetZone<IHandZone<WeissCard>>();
            if (hand == null) return false;

            return hand.Cards.Count(card => _filter(card)) >= _amount;
        }

        /// <summary>
        /// 実際にコストを支払います。プレイヤーに捨てさせるカードを選択させ、手札から控え室に移動します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            if (!CanPay(state, player, source)) return;

            var hand = player.GetZone<IHandZone<WeissCard>>();
            var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
            var controller = (player as WeissPlayer)?.Controller;

            if (hand == null || waitingRoom == null || controller == null) return;

            // 1. 捨てることができるカードのリストを作成する
            var validCardsToDiscard = hand.Cards.Where(card => _filter(card)).ToList();

            // 2. プレイヤーコントローラーに、どのカードをコストとして支払うか選択させる
            string reason = $"コストとして手札から{_amount}枚のカードを選んでください。";
            var cardsToDiscard = controller.SelectCardsToPayCost(player as WeissPlayer, validCardsToDiscard, _amount, reason);

            // 3. 選択されたカードを手札から控え室に移動する
            if (cardsToDiscard != null && cardsToDiscard.Count == _amount)
            {
                foreach (var card in cardsToDiscard)
                {
                    hand.RemoveCard(card);
                    waitingRoom.AddCard(card);
                }
            }
            // else: プレイヤーが正しい枚数のカードを選択しなかった場合の処理。
            // CanPayがtrueなら、シミュレーション用のコントローラーは常に正しい選択をすると想定している。
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            // TODO: filterの内容を反映した、より正確な説明文を生成する
            return $"手札を{_amount}枚控え室に置く";
        }
    }
}