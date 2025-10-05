using System.Linq;
using TCG.Core;

namespace TCG.Core.Costs
{
    public class StockCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;
        public StockCost(int amount) => this.amount = amount;

        public bool CanPay(GameState state, Player player)
        {
            var stock = player.GetZone<IStockZone<TCard>>();
            return stock != null && stock.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player)
        {
            var stock = player.GetZone<IStockZone<TCard>>();
            var discardPile = player.GetZone<IDiscardPile<TCard>>();

            if (stock == null || discardPile == null) return; // Or throw an exception

            for (int i = 0; i < amount; i++)
            {
                if (stock.Cards.Count == 0) break;
                var card = stock.Cards.First();
                stock.RemoveCard(card);
                discardPile.AddCard(card);
            }
        }
    }
}
