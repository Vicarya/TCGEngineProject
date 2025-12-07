using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// プレイヤーの入力や意思決定を抽象化するインターフェース。
    /// これを実装することで、UIからの入力、AIの思考ルーチン、コンソールからの入力など、様々な操作方法に対応できます。
    /// </summary>
    /// <typeparam name="TPlayer">プレイヤーの型。</typeparam>
    /// <typeparam name="TCard">カードの型。</typeparam>
    public interface IPlayerController<TPlayer, TCard>
    {
        /// <summary>
        /// プレイヤーに手札からカードを1枚選ばせます。
        /// </summary>
        /// <param name="player">選択を行うプレイヤー。</param>
        /// <param name="optional">選択が任意であるか（キャンセル可能か）どうか。</param>
        /// <returns>プレイヤーが選んだカード。選択されなかった場合はnull。</returns>
        TCard ChooseCardFromHand(TPlayer player, bool optional);

        /// <summary>
        /// プレイヤーに攻撃するキャラクターを1体選ばせます。
        /// </summary>
        /// <param name="player">選択を行うプレイヤー。</param>
        /// <param name="attackableCharacters">攻撃可能なキャラクターのリスト。</param>
        /// <returns>プレイヤーが選んだ攻撃キャラクター。</returns>
        TCard ChooseAttacker(TPlayer player, List<TCard> attackableCharacters);

        /// <summary>
        /// プレイヤーにアタックフェイズを終了するかどうかを選ばせます。
        /// </summary>
        /// <param name="player">選択を行うプレイヤー。</param>
        /// <returns>アタックを終了する場合はtrue、続ける場合はfalse。</returns>
        bool ChooseToEndAttack(TPlayer player);

        /// <summary>
        /// プレイヤーにマリガン（手札の引き直し）で交換するカードを選ばせます。
        /// </summary>
        /// <param name="player">選択を行うプレイヤー。</param>
        /// <param name="hand">現在の初期手札。</param>
        /// <returns>交換対象として選ばれたカードのリスト。</returns>
        List<TCard> ChooseMulliganCards(TPlayer player, List<TCard> hand);
    }
}
