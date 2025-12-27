using System.Collections.Generic;
using System.Linq;
using TCG.Core;
using TCG.Weiss;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// ヴァイスシュヴァルツのキーワード能力「集中」の効果を処理することを意図したクラス。
    /// TCG.Core.IEffectインターフェースを実装しています。
    /// 【重要】現在、このクラスの実装は完了しておらず、ロジックはすべてコメントアウトされています。
    /// </summary>
    public class ConcentrateEffect : IEffect
    {
        /// <summary>
        /// 集中の効果を解決します。
        /// 【未実装】現在、このメソッドの中身は実装されていません。
        /// </summary>
        /// <param name="e">この効果をトリガーしたゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="source">この効果の発生源であるカード。</param>
        public void Resolve(GameEvent e, GameState state, Card source)
        {
            var player = state.ActivePlayer as WeissPlayer;
            if (player == null) return;

            // NOTE: 以下は「集中」の一般的な処理（山札の上から4枚めくり、クライマックスの枚数分ドローする）を
            //       実装しようとした痕跡です。
            //       実際のゲームロジックとして機能させるには、コメントを解除し、
            //       WeissRuleEngineのメソッド（DrawCardなど）を介した形に正しく再実装する必要があります。

            // TODO: player.Deck は WeissPlayer にあるべきプロパティ。一旦仮定。
            // var deck = player.Deck;
            // var discarded = new List<Card>();

            // for (int i = 0; i < 4; i++)
            // {
            //     var card = deck.DrawTop();
            //     if (card != null) discarded.Add(card);
            // }

            // int climaxCount = discarded.Count(c => (c.Data as WeissCardData)?.IsClimax ?? false);
            // for (int i = 0; i < climaxCount; i++)
            // {
            //     // TODO: Player.Draw() は存在しない。RuleEngineなどを介して実装する必要がある。
            //     // player.Draw(1);
            // }
        }
    }
}
