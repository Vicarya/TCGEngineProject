using System;
using System.Collections.Generic;
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    public class UIGamePlayerController : IWeissPlayerController
    {
        public void ResetTurnState()
        {
            Debug.Log("UIGamePlayerController: ResetTurnState called.");
            // UI-specific reset logic here
        }

        public MainPhaseAction ChooseMainPhaseAction(WeissPlayer player)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}'s Main Phase. Waiting for UI input.");
            // This method should ideally block and wait for UI input.
            // For now, return a default action.
            return MainPhaseAction.EndPhase;
        }

        public KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose an ability to activate. Waiting for UI input.");
            return default;
        }

        public WeissCard ChooseCardFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a card from hand. Waiting for UI input.");
            return null;
        }

        public WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a climax card from hand. Waiting for UI input.");
            return null;
        }

        public bool ChooseToEndAttack(WeissPlayer player)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Continue attacking? Waiting for UI input.");
            return true; // Simulate ending attack for now
        }

        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose an attacker. Waiting for UI input.");
            return null;
        }

        public AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose attack type. Waiting for UI input.");
            return AttackType.Front; // Simulate Front Attack
        }

        public List<WeissCard> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}'s Mulligan Phase. Waiting for UI input.");
            return new List<WeissCard>();
        }

        public WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a card from waiting room. Waiting for UI input.");
            return null;
        }

        public WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose counter card. Waiting for UI input.");
            return null;
        }

        public EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose encore option. Waiting for UI input.");
            return EncoreChoice.None;
        }

        public WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose level up card. Waiting for UI input.");
            return null;
        }

        public bool AskYesNo(WeissPlayer player, string question)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Ask Yes/No: {question}. Waiting for UI input.");
            return false; // Simulate No for now
        }

        public WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose target card: {prompt}. Waiting for UI input.");
            return null;
        }

        public PendingAbility ChooseAbilityToResolve(WeissPlayer player, List<PendingAbility> abilities)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose ability order. Waiting for UI input.");
            return default;
        }

        public List<WeissCard> SelectCardsToPayCost(WeissPlayer player, List<WeissCard> options, int amount, string reason)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Select cards for cost: {reason}. Waiting for UI input.");
            return new List<WeissCard>();
        }
    }
}