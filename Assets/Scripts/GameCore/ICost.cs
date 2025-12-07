namespace TCG.Core
{
    /// <summary>
    /// カードの能力やイベントを使用する際に支払うべきコストを抽象化するインターフェース。
    /// これを実装することで、ストックコスト、手札破壊コストなど、ゲーム固有の様々なコストを表現できます。
    /// </summary>
    public interface ICost
    {
        /// <summary>
        /// 指定された状態でコストが支払い可能かどうかを判定します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能であればtrue、そうでなければfalse。</returns>
        bool CanPay(GameState state, Player player, Card source);

        /// <summary>
        /// コストを実際に支払います。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        void Pay(GameState state, Player player, Card source);

        /// <summary>
        /// コストの内容を説明する文字列を取得します。
        /// （例：「【コスト】ストックを(2)支払う」）
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        string GetDescription();
    }
}

// Ability 側での利用例
// var cost = new Costs()
//     .Add(new StockCost(2))
//     .Add(new DiscardHandCost(1));

// if (cost.CanPay(state, player, sourceCard))
// {
//     cost.Pay(state, player, sourceCard);
//     // 効果解決へ進む
// }
