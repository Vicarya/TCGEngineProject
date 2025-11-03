using TCG.Core;
using System;

namespace TCG.Weiss {
    public class WeissCard : CardBase<WeissCardData>
    {
        public bool IsReversed { get; private set; }
        public int TemporaryPower { get; set; }

        public WeissCard(WeissCardData data, Player owner) : base(data, owner) 
        {
            IsReversed = false;
            TemporaryPower = 0;

            var createdAbilities = AbilityFactory.CreateAbilitiesForCard(this);
            foreach (var ability in createdAbilities)
            {
                this.AddAbility(ability);
            }
        }

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

        public void SetReversed(bool reversed)
        {
            IsReversed = reversed;
        }
    }
}
