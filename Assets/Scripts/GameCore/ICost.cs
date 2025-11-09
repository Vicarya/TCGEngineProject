namespace TCG.Core
{
    /// <summary>
    /// コストの支払いフローを抽象化するインターフェース
    /// </summary>
    public interface ICost
    {
        bool CanPay(GameState state, Player player, Card source);
        void Pay(GameState state, Player player, Card source);
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
