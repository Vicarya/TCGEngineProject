
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public class ConsolePlayerController : IWeissPlayerController
    {
        public MainPhaseAction ChooseMainPhaseAction(WeissPlayer player)
        {
            // TODO: Implement actual user input
            return MainPhaseAction.EndPhase;
        }

        public WeissCard ChooseCardFromHand(WeissPlayer player, bool optional)
        {
            // TODO: Implement actual user input
            return null;
        }

        public WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional)
        {
            // TODO: Implement actual user input
            return null;
        }

        public bool ChooseToEndAttack(WeissPlayer player)
        {
            // TODO: Implement actual user input
            return false; // Always attack if possible
        }

        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            // TODO: Implement actual user input
            return attackableCharacters.FirstOrDefault();
        }

        public AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender)
        {
            // TODO: Implement actual user input
            return AttackType.Front;
        }

        public List<WeissCard> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand)
        {
            // TODO: Implement actual user input for mulligan
            return new List<WeissCard>(); // Do not mulligan
        }
    }
}
