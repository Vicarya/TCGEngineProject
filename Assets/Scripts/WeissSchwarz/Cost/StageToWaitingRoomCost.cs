using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class StageToWaitingRoomCost<TCard> : ICost where TCard : Card
    {
        private readonly int amount;

        public StageToWaitingRoomCost(int amount)
        {
            this.amount = amount;
        }

        public bool CanPay(GameState state, Player player, Card source)
        {
            var stageZone = player.GetZone<IStageZone<TCard>>();
            return stageZone != null && stageZone.Cards.Count >= amount;
        }

        public void Pay(GameState state, Player player, Card source)
        {
            var stageZone = player.GetZone<IStageZone<TCard>>();
            var discardPile = player.GetZone<IDiscardPile<TCard>>();

            if (stageZone == null || discardPile == null) return; // Or throw an exception

            for (int i = 0; i < amount; i++)
            {
                if (stageZone.Cards.Count == 0) break;
                // For simplicity, just take the first card. In a real game, selection might be needed.
                var card = stageZone.Cards.First(); 
                stageZone.RemoveCard(card);
                discardPile.AddCard(card);
            }
        }

        public string GetDescription()
        {
            return $"舞台のカードを{amount}枚控え室に置く";
        }
    }
}
