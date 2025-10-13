using TCG.Core;
using TCG.Weiss;

namespace TCG.Weiss
{
    /// <summary>
    /// 自身をレスト状態にするコスト
    /// </summary>
    public class RestSelfCost<TCard> : ICost where TCard : Card
    {
        private readonly TCard source;

        public RestSelfCost(TCard source)
        {
            this.source = source;
        }

        public bool CanPay(GameState state, Player player)
        {
            // すでにレストしているなら支払不可
            return source != null && !source.IsRested;
        }

        public void Pay(GameState state, Player player)
        {
            if (source != null) source.SetRested(true);
        }
    }
}
