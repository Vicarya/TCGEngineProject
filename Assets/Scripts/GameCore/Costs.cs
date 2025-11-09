using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Core
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

        public Costs AddRange(IEnumerable<ICost> costs)
        {
            if (costs == null) return this;
            foreach (var c in costs)
            {
                if (c != null) costList.Add(c);
            }
            return this;
        }

        public bool CanPay(GameState state, Player player, Card source)
        {
            return costList.All(cost => cost.CanPay(state, player, source));
        }

        public void Pay(GameState state, Player player, Card source)
        {
            foreach (var cost in costList)
            {
                cost.Pay(state, player, source);
            }
        }

        public IEnumerator<ICost> GetEnumerator()
        {
            return costList.GetEnumerator();
        }

        public int Count => costList.Count;
    }
}
