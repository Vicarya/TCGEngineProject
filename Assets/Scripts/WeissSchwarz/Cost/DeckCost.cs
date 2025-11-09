using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class DeckCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        public DeckCost(int amount)
        {
            this.amount = amount;
        }

        public bool CanPay(GameState state, Player player, Card source)
        {
            var deckZone = player.GetZone<IDeckZone<TCard>>();
            return deckZone != null && deckZone.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player, Card source)
        {
            var deckZone = player.GetZone<IDeckZone<TCard>>();
            var handZone = player.GetZone<IHandZone<TCard>>();

            if (deckZone == null || handZone == null) return; // Or throw an exception

            for (int i = 0; i < amount; i++)
            {
                if (deckZone.Cards.Count == 0) break;
                var card = deckZone.Cards.First(); // Assuming drawing from top of deck
                deckZone.RemoveCard(card);
                handZone.AddCard(card);
            }
        }

        public string GetDescription()
        {
            return $"Draw {amount} card(s) from deck.";
        }
    }
}