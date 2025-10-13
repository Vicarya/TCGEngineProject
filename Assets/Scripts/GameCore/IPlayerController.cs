using System.Collections.Generic;

namespace TCG.Core
{
    public interface IPlayerController<TPlayer, TCard>
    {
        TCard ChooseCardFromHand(TPlayer player, bool optional);
        TCard ChooseAttacker(TPlayer player, List<TCard> attackableCharacters);
        bool ChooseToEndAttack(TPlayer player);
        List<TCard> ChooseMulliganCards(TPlayer player, List<TCard> hand);
    }
}
