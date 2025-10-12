using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    public interface IPlayerController
    {
        MainPhaseAction ChooseMainPhaseAction(Player player);
        WeissCard ChooseCardFromHand(Player player, bool optional);
        WeissCard ChooseClimaxFromHand(Player player, bool optional);
        bool ChooseToEndAttack(Player player);
        WeissCard ChooseAttacker(Player player, List<WeissCard> attackableCharacters);
        AttackType ChooseAttackType(Player player, WeissCard attacker, WeissCard defender);
    }
}