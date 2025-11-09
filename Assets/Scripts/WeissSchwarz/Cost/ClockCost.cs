using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class ClockCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        public ClockCost(int amount)
        {
            this.amount = amount;
        }

        public bool CanPay(GameState state, Player player, Card source)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            return hand != null && hand.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player, Card source)
        {
            var hand = player.GetZone<IHandZone<TCard>>();
            var clockZone = player.GetZone<IClockZone<TCard>>();

            if (hand == null || clockZone == null) return; // Or throw an exception

            for (int i = 0; i < amount; i++)
            {
                if (hand.Cards.Count == 0) break;
                var card = hand.Cards.First();
                hand.RemoveCard(card);
                clockZone.AddCard(card);
            }
        }

        public string GetDescription()
        {
            return $"クロックを{amount}枚払う";
        }
    }
}
