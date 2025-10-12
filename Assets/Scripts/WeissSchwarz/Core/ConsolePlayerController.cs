
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class ConsolePlayerController : IPlayerController
    {
        public MainPhaseAction ChooseMainPhaseAction(Player player)
        {
            // TODO: Implement actual user input
            return MainPhaseAction.EndPhase;
        }

        public WeissCard ChooseCardFromHand(Player player, bool optional)
        {
            // TODO: Implement actual user input
            return null;
        }

        public WeissCard ChooseClimaxFromHand(Player player, bool optional)
        {
            // TODO: Implement actual user input
            return null;
        }

        public bool ChooseToEndAttack(Player player)
        {
            // TODO: Implement actual user input
            return false; // Always attack if possible
        }

        public WeissCard ChooseAttacker(Player player, List<WeissCard> attackableCharacters)
        {
            // TODO: Implement actual user input
            return attackableCharacters.FirstOrDefault();
        }

        public AttackType ChooseAttackType(Player player, WeissCard attacker, WeissCard defender)
        {
            // TODO: Implement actual user input
            return AttackType.Front;
        }
    }
}
