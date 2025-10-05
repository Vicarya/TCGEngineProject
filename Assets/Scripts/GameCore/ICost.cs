namespace TCG.Core
{
    /// <summary>
    /// コストの支払いフローを抽象化するインターフェース
    /// </summary>
    public interface ICost
    {
        bool CanPay(GameState state, Player player);
        void Pay(GameState state, Player player);
    }
}

// Ability 側での利用例
// var cost = new Costs()
//     .Add(new StockCost(2))
//     .Add(new DiscardHandCost(1));

// if (cost.CanPay(state, player))
// {
//     cost.Pay(state, player);
//     // 効果解決へ進む
// }
