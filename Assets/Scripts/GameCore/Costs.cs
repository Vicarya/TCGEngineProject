using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace Costs
{
    public class Costs
    {
        private readonly List<ICost> costList = new();

        public bool IsEmpty => costList.Count == 0;

        public Costs Add(ICost cost)
        {
            costList.Add(cost);
            return this;
        }

        public bool CanPay(GameState state, Player player)
        {
            return costList.All(cost => cost.CanPay(state, player));
        }

        public void Pay(GameState state, Player player)
        {
            foreach (var cost in costList)
            {
                cost.Pay(state, player);
            }
        }
    }
}
