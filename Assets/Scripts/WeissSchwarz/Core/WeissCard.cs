using TCG.Core;
using System;

namespace TCG.Weiss {
    public class WeissCard : CardBase<WeissCardData>
    {
        public WeissCard(WeissCardData data, Player owner) : base(data, owner) { }

        public void Rest()
        {
            SetRested(true);
            IsTapped = true;
        }

        public void Stand()
        {
            SetRested(false);
            IsTapped = false;
        }
    }
}
