
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

        public WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional)
        {
            Debug.Log("Choose a card from the waiting room:");
            if (cards == null || !cards.Any())
            {
                Debug.Log("No cards to choose from.");
                return null;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                Debug.Log($"{i + 1}: [{card.Data.CardCode}] {card.Data.Name}");
            }

            if (optional)
            {
                Debug.Log("Auto-selecting: None (optional)");
                return null;
            }
            
            Debug.Log("Auto-selecting first card.");
            return cards.FirstOrDefault();
        }

        public WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards)
        {
            Debug.Log($"--- {player.Name}'s Counter Step ---");
            if (counterCards == null || !counterCards.Any())
            {
                Debug.Log("No counter cards in hand.");
                return null;
            }

            Debug.Log("Available counter cards:");
            for (int i = 0; i < counterCards.Count; i++)
            {
                var card = counterCards[i];
                Debug.Log($"{i + 1}: [{card.Data.CardCode}] {card.Data.Name}");
            }

            Debug.Log("Auto-selecting: Do not play a counter.");
            return null;
        }

        public EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character)
        {
            Debug.Log($"--- {player.Name}'s Encore Step for [{character.Data.Name}] ---");

            // Check for special encore ability (e.g., 【自】 アンコール ［...］)
            bool hasSpecialEncore = false;
            string specialEncoreText = null;
            if (character.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
            {
                if (abilitiesObj is List<string> abilities)
                {
                    specialEncoreText = abilities.FirstOrDefault(text => text.StartsWith("【自】 アンコール"));
                    if (specialEncoreText != null)
                    {
                        hasSpecialEncore = true;
                    }
                }
            }

            if (hasSpecialEncore)
            {
                // A real implementation would parse the cost and ask the player.
                Debug.Log($"Card has a special encore ability: '{specialEncoreText}'. Auto-selecting: Special Encore.");
                // Here, we should check if the cost can be paid. For now, we just assume it can.
                return EncoreChoice.Special;
            }

                // Standard encore is always an option for characters, even if not written on the card.
                int stockCount = player.GetZone<IStockZone<WeissCard>>().Cards.Count;
                if (stockCount >= 3)
                {
                    Debug.Log($"Player has {stockCount} stock. Standard encore is available (cost: 3). Auto-selecting: Standard Encore.");                return EncoreChoice.Standard;
            }

            Debug.Log("Not enough stock for standard encore and no special encore available. Auto-selecting: None.");
            return EncoreChoice.None;
        }
    }
}
