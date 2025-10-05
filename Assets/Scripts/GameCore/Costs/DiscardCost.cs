using System.Linq;
using TCG.Core;

namespace TCG.Core.Costs
{
    public class DiscardCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;
        public DiscardCost(int amount) => this.amount = amount;

        public bool CanPay(GameState state, Player player)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            return hand != null && hand.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            var discardPile = player.GetZone<IDiscardPile<TCard>>();

            if (hand == null || discardPile == null) return; // Or throw an exception

            for (int i = 0; i < amount; i++)
            {
                // TODO: 実際にはユーザー選択が必要（ここでは先頭を仮選択）
                if (hand.Cards.Count == 0) break;
                var card = hand.Cards.First();
                hand.RemoveCard(card);
                discardPile.AddCard(card);
            }
        }
    }
}
