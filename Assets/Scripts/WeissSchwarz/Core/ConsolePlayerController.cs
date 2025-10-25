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
        private int _attacksChosenThisTurn = 0;

        public void ResetTurnState()
        {
            _hasAutoUsedAbilityThisTurn = false;
            _attacksChosenThisTurn = 0;
        }

        public MainPhaseAction ChooseMainPhaseAction(WeissPlayer player)
        {
            Debug.Log($"--- {player.Name}'s Main Phase ---");
            Debug.Log("Choose an action:");
            Debug.Log("1: Play a card from hand");
            Debug.Log("2: Use an activate ability");
            Debug.Log("3: End Main Phase");

            if (!_hasAutoUsedAbilityThisTurn)
            {
                _hasAutoUsedAbilityThisTurn = true;
                Debug.Log("Simulating player input: 2 (Use Ability)");
                return MainPhaseAction.UseAbility;
            }
            
            Debug.Log("Simulating player input: 3 (End Phase)");
            return MainPhaseAction.EndPhase;
        }

        public KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities)
        {
            Debug.Log("--- " + player.Name + ": Choose an ability to activate ---");
            if (activatableAbilities == null || !activatableAbilities.Any())
            {
                Debug.Log("No activatable abilities.");
                return default;
            }

            for (int i = 0; i < activatableAbilities.Count; i++)
            {
                var ability = activatableAbilities[i];
                Debug.Log($"{i + 1}: [{ability.Key.Data.CardCode} {ability.Key.Data.Name}] - {ability.Value}");
            }
            Debug.Log($"{activatableAbilities.Count + 1}: Do not activate an ability");

            Debug.Log("Enter the number of the ability to activate:");

            var choice = activatableAbilities.FirstOrDefault();
            Debug.Log($"Simulating player input: 1 ({choice.Value})");

            return choice;
        }

        public WeissCard ChooseCardFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"--- {player.Name}: Choose a card from hand ---");
            var hand = player.GetZone<IHandZone<WeissCard>>().Cards;

            if (hand == null || !hand.Any())
            {
                Debug.Log("No cards in hand.");
                return null;
            }

            Debug.Log("Your hand:");
            for (int i = 0; i < hand.Count; i++)
            {
                Debug.Log($"{i + 1}: [{hand[i].Data.CardCode}] {hand[i].Data.Name}");
            }

            string prompt = "Enter the number of the card to choose";
            if (optional)
            {
                prompt += " (or press Enter to choose none)";
            }
            Debug.Log(prompt + ":");

            if (optional)
            {
                Debug.Log("Simulating player input: Choosing None.");
                return null;
            }
            else
            {
                var choice = hand.First();
                Debug.Log($"Simulating player input: Choosing first card [{choice.Data.Name}]");
                return choice;
            }
        }

        public WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"--- {player.Name}: Choose a climax card from hand ---");
            var hand = player.GetZone<IHandZone<WeissCard>>().Cards;
            var climaxCards = hand.Where(c => ((c.Data as WeissCardData)?.CardType == WeissCardType.Climax.ToString())).ToList();

            if (!climaxCards.Any())
            {
                Debug.Log("No climax cards in hand.");
                return null;
            }

            Debug.Log("Your climax cards:");
            for (int i = 0; i < climaxCards.Count; i++)
            {
                Debug.Log($"{i + 1}: [{climaxCards[i].Data.CardCode}] {climaxCards[i].Data.Name}");
            }

            Debug.Log("Enter the number of the climax to play (or press Enter to choose none):");

            Debug.Log("Simulating player input: Choosing None.");
            return null;
        }

        public bool ChooseToEndAttack(WeissPlayer player)
        {
            Debug.Log("--- " + player.Name + ": Continue attacking? ---");
            Debug.Log("1: Perform an attack");
            Debug.Log("2: End Attack Phase");

            if (_attacksChosenThisTurn < 1)
            {
                Debug.Log("Simulating player input: 1 (Perform an attack)");
                _attacksChosenThisTurn++;
                return false; 
            }
            
            Debug.Log("Simulating player input: 2 (End Attack Phase)");
            return true; 
        }

        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            Debug.Log($"--- {player.Name}: Choose an attacker ---");
            if (attackableCharacters == null || !attackableCharacters.Any())
            {
                Debug.Log("No characters available to attack.");
                return null;
            }

            Debug.Log("Your attackable characters:");
            for (int i = 0; i < attackableCharacters.Count; i++)
            {
                Debug.Log($"{i + 1}: [{attackableCharacters[i].Data.CardCode}] {attackableCharacters[i].Data.Name} (Power: {attackableCharacters[i].Data.Power})");
            }

            Debug.Log("Enter the number of the character to attack with:");

            var choice = attackableCharacters.First();
            Debug.Log($"Simulating player input: Choosing [{choice.Data.Name}]");

            return choice;
        }

        public AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender)
        {
            Debug.Log($"--- {player.Name}: Choose attack type for [{attacker.Data.Name}] ---");
            Debug.Log($"Opponent's character is [{defender.Data.Name}] (Power: {defender.Data.Power})");
            Debug.Log("Choose an attack type:");
            Debug.Log("1: Front Attack");
            Debug.Log("2: Side Attack");

            Debug.Log("Simulating player input: 1 (Front Attack)");
            return AttackType.Front;
        }

        public List<WeissCard> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand)
        {
            Debug.Log($"--- {player.Name}'s Mulligan Phase ---");
            if (hand == null || !hand.Any())
            {
                Debug.Log("No cards in hand to mulligan.");
                return new List<WeissCard>();
            }

            Debug.Log("Your hand:");
            for (int i = 0; i < hand.Count; i++)
            {
                Debug.Log($"{i + 1}: [{hand[i].Data.CardCode}] {hand[i].Data.Name}");
            }

            Debug.Log("Enter the numbers of cards to mulligan (e.g., '1 3 5'), or press Enter to keep your hand:");

            var cardsToMulligan = new List<WeissCard>();
            var climaxInHand = hand.FirstOrDefault(c => ((c.Data as WeissCardData)?.CardType == WeissCardType.Climax.ToString()));

            if (climaxInHand != null)
            {
                Debug.Log($"Simulating player input: Mulliganning one climax card: [{climaxInHand.Data.Name}]");
                cardsToMulligan.Add(climaxInHand);
            }
            else
            {
                Debug.Log("Simulating player input: Keeping hand.");
            }

            return cardsToMulligan;
        }

        public WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional)
        {
            Debug.Log($"--- {player.Name}: Choose a card from waiting room ---");
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

            string prompt = "Enter the number of the card to choose";
            if (optional)
            {
                prompt += " (or press Enter to choose none)";
            }
            Debug.Log(prompt + ":");

            if (optional)
            {
                Debug.Log("Simulating player input: Choosing None.");
                return null;
            }
            else
            {
                var choice = cards.First();
                Debug.Log($"Simulating player input: Choosing first card [{choice.Data.Name}]");
                return choice;
            }
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

            Debug.Log("Enter the number of the card to play (or press Enter to play none):");

            Debug.Log("Simulating player input: Choosing None.");
            return null;
        }

        public EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character)
        {
            Debug.Log($"--- {player.Name}'s Encore Step for [{character.Data.Name}] ---");

            var options = new List<KeyValuePair<EncoreChoice, string>>();

            string specialEncoreText = null;
            if (character.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
            {
                if (abilitiesObj is List<string> abilities)
                {
                    specialEncoreText = abilities.FirstOrDefault(text => text.StartsWith("【自】 アンコール"));
                    if (specialEncoreText != null)
                    {
                        options.Add(new KeyValuePair<EncoreChoice, string>(EncoreChoice.Special, $"Special Encore: {specialEncoreText}"));
                    }
                }
            }

            int stockCount = player.GetZone<IStockZone<WeissCard>>().Cards.Count;
            if (stockCount >= 3)
            {
                options.Add(new KeyValuePair<EncoreChoice, string>(EncoreChoice.Standard, $"Standard Encore (Pay 3 stock)"));
            }

            if (!options.Any())
            {
                Debug.Log("No encore options available.");
                return EncoreChoice.None;
            }

            Debug.Log("Choose an encore option:");
            for (int i = 0; i < options.Count; i++)
            {
                Debug.Log($"{i + 1}: {options[i].Value}");
            }
            Debug.Log($"{options.Count + 1}: Do not encore");

            if (options.Any(o => o.Key == EncoreChoice.Special))
            {
                Debug.Log("Simulating player input: 1 (Special Encore)");
                return EncoreChoice.Special;
            }
            if (options.Any(o => o.Key == EncoreChoice.Standard))
            {
                Debug.Log("Simulating player input: 1 (Standard Encore)");
                return EncoreChoice.Standard;
            }

            Debug.Log("Simulating player input: Choosing None.");
            return EncoreChoice.None;
        }

        public WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards)
        {
            Debug.Log($"--- {player.Name}'s Level Up! ---");
            Debug.Log("Choose 1 card from your clock to level up with:");
            for (int i = 0; i < cards.Count; i++)
            {
                Debug.Log($"{i + 1}: [{cards[i].Data.CardCode}] {cards[i].Data.Name}");
            }

            Debug.Log("Enter the number of the card to place in your level zone:");

            var choice = cards.FirstOrDefault(c => ((c.Data as WeissCardData)?.CardType != WeissCardType.Climax.ToString())) ?? cards.First();
            Debug.Log($"Simulating player input: Choosing [{choice.Data.Name}]");

            return choice;
        }

        public bool AskYesNo(WeissPlayer player, string question)
        {
            Debug.Log($"--- {player.Name}: Decision ---");
            Debug.Log(question);
            Debug.Log("1: Yes");
            Debug.Log("2: No");

            Debug.Log("Simulating player input: 1 (Yes)");
            return true;
        }

        public WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional)
        {
            Debug.Log($"--- {player.Name}: Target Selection ---");
            Debug.Log(prompt);

            if (validTargets == null || !validTargets.Any())
            {
                Debug.Log("No valid targets.");
                return null;
            }

            for (int i = 0; i < validTargets.Count; i++)
            {
                Debug.Log($"{i + 1}: [{validTargets[i].Data.CardCode}] {validTargets[i].Data.Name}");
            }

            string inputPrompt = "Enter the number of the card to target";
            if (optional)
            {
                inputPrompt += " (or press Enter to choose none)";
            }
            Debug.Log(inputPrompt + ":");

            if (optional)
            {
                Debug.Log("Simulating player input: Choosing None.");
                return null;
            }
            else
            {
                var choice = validTargets.First();
                Debug.Log($"Simulating player input: Choosing first target [{choice.Data.Name}]");
                return choice;
            }
        }

        public PendingAbility ChooseAbilityToResolve(WeissPlayer player, List<PendingAbility> abilities)
        {
            Debug.Log($"--- {player.Name}: Choose ability order ---");
            Debug.Log("Multiple abilities have triggered. Choose the one to resolve first:");

            for (int i = 0; i < abilities.Count; i++)
            {
                Debug.Log($"{i + 1}: {abilities[i]}");
            }

            var choice = abilities.First();
            Debug.Log($"Simulating player input: 1 (Resolving '{choice.AbilityText}' first)");

            return choice;
        }
    }
}
