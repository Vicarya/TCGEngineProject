
using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    public class ConsolePlayerController : IWeissPlayerController
    {
        private bool _hasAutoUsedAbilityThisTurn = false;

        public void ResetTurnState()
        {
            _hasAutoUsedAbilityThisTurn = false;
        }

        public MainPhaseAction ChooseMainPhaseAction(WeissPlayer player)
        {
            Debug.Log($"--- {player.Name}'s Main Phase ---");

            if (!_hasAutoUsedAbilityThisTurn)
            {
                _hasAutoUsedAbilityThisTurn = true;
                Debug.Log("Auto-selecting: Use Ability");
                return MainPhaseAction.UseAbility;
            }
            
            Debug.Log("Auto-selecting: End Phase");
            return MainPhaseAction.EndPhase;
        }

        public KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities)
        {
            Debug.Log("Choose an ability to activate:");
            if (activatableAbilities == null || !activatableAbilities.Any())
            {
                Debug.Log("No activatable abilities.");
                return default;
            }

            for (int i = 0; i < activatableAbilities.Count; i++)
            {
                var ability = activatableAbilities[i];
                // Corrected: Access the card data via the 'Data' property.
                Debug.Log($"{i + 1}: [{ability.Key.Data.CardCode} {ability.Key.Data.Name}] - {ability.Value}");
            }
            
            Debug.Log("Auto-selecting first ability.");
            return activatableAbilities.FirstOrDefault();
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
            return true; // End attack phase immediately for now
        }

        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            // TODO: Implement actual user input
            return null;
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
