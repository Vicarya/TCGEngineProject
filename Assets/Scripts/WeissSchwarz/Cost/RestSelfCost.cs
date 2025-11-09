using TCG.Core;
using TCG.Weiss;

namespace TCG.Weiss
{
    /// <summary>
    /// 自身をレスト状態にするコスト
    /// </summary>
    public class RestSelfCost<TCard> : ICost where TCard : Card
    {
        private readonly TCard card;

        public RestSelfCost(TCard card)
        {
            this.card = card;
        }

        // ICost requires a source Card parameter. We accept the passed-in source if provided,
        // otherwise fall back to the card stored on construction.
        public bool CanPay(GameState state, Player player, Card source)
        {
            var target = source as TCard ?? card;
            // すでにレストしているなら支払不可
            return target != null && !target.IsRested;
        }

        public void Pay(GameState state, Player player, Card source)
        {
            var target = source as TCard ?? card;
            if (target != null) target.SetRested(true);
        }

        public string GetDescription()
        {
            return "Rest this card.";
        }
    }
}
