using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class DrawCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        public DrawCost(int amount)
        {
            this.amount = amount;
        }

        public bool CanPay(GameState state, Player player)
        {
            var deck = player.GetZone<IDeckZone<TCard>>();
            return deck != null && deck.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player)
        {
            var deck = player.GetZone<IDeckZone<TCard>>();
            var hand = player.GetZone<IHandZone<TCard>>();

            if (deck == null || hand == null) return;

            for (int i = 0; i < amount; i++)
            {
                if (deck.Cards.Count == 0) break;
                var card = deck.Cards.First();
                deck.RemoveCard(card);
                hand.AddCard(card);
            }
        }
    }
}
