using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss 
{
    public class WeissRuleEngine {
        public WeissGameState GameState { get; }
        public PhaseBase TurnPhaseTree { get; }

        private WeissGameState _weissState => GameState;

        public WeissRuleEngine(GameState state) {
            GameState = state as WeissGameState;
            if (GameState == null)
            {
                Debug.LogError("WeissRuleEngine requires a WeissGameState!");
                return;
            }
            TurnPhaseTree = WeissPhaseFactory.CreateTurnPhaseTree();
            GameState.EventBus.SubscribeToAll(HandleGameEvent);
        }

        private void HandleGameEvent(GameEvent evt)
        {
            CheckForTriggeredAbilities(evt);
            ResolveAbilityQueue();
        }

        public void CheckForTriggeredAbilities(GameEvent evt)
        {
            if (_weissState == null) return;

            if (evt.Type == BaseGameEvents.CardPlayed)
            {
                if (evt.Data is CardPlayedEventArgs onPlayData)
                {
                    WeissCard playedCard = onPlayData.Card;
                    WeissPlayer player = onPlayData.Player;

                    if (playedCard.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
                    {
                        if (abilitiesObj is List<string> abilities)
                        {
                            foreach (var abilityText in abilities)
                            {
                                if (abilityText.StartsWith("【自】 このカードが手札から舞台に置かれた時"))
                                {
                                    Debug.Log($"Triggered on-play ability for {playedCard.Data.Name}: {abilityText}");
                                    var pendingAbility = new PendingAbility(playedCard, abilityText, player);
                                    _weissState.AbilityQueue.Add(pendingAbility);
                                }
                            }
                        }
                    }
                }
            }
            else if (evt.Type == WeissGameEvents.AttackDeclared)
            {
                if (evt.Data is AttackDeclaredEventArgs onAttackData)
                {
                    WeissCard attacker = onAttackData.Attacker;
                    WeissPlayer player = attacker.Owner as WeissPlayer;

                    if (attacker.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
                    {
                        if (abilitiesObj is List<string> abilities)
                        {
                            foreach (var abilityText in abilities)
                            {
                                if (abilityText.StartsWith("【自】 このカードがアタックした時"))
                                {
                                    Debug.Log($"Triggered on-attack ability for {attacker.Data.Name}: {abilityText}");
                                    var pendingAbility = new PendingAbility(attacker, abilityText, player);
                                    _weissState.AbilityQueue.Add(pendingAbility);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ResolveAbilityQueue()
        {
            if (_weissState == null) return;

            while (true)
            {
                var activePlayer = _weissState.ActivePlayer as WeissPlayer;
                var pendingAbilities = _weissState.AbilityQueue.GetPendingAbilitiesForPlayer(activePlayer);

                if (!pendingAbilities.Any())
                {
                    break; 
                }

                PendingAbility abilityToResolve;
                if (pendingAbilities.Count > 1)
                {
                    abilityToResolve = activePlayer.Controller.ChooseAbilityToResolve(activePlayer, pendingAbilities);
                }
                else
                {
                    abilityToResolve = pendingAbilities.First();
                }

                ResolveAbilityEffect(abilityToResolve);

                _weissState.AbilityQueue.Remove(abilityToResolve);
            }
        }

        private void ResolveAbilityEffect(PendingAbility ability)
        {
            Debug.Log($"Resolving effect for: {ability.AbilityText}");

            const string onPlayStockAbility = "【自】 このカードが手札から舞台に置かれた時、あなたは自分の山札の上から1枚を、ストック置場に置いてよい。";

            if (ability.AbilityText == onPlayStockAbility)
            {
                var player = ability.ResolvingPlayer;

                bool useAbility = player.Controller.AskYesNo(player, $"Use ability? \"{ability.AbilityText}\" ");
                if (useAbility)
                {
                    var stock = player.GetZone<IStockZone<WeissCard>>();
                    var cardToStock = this.DrawCard(player);
                    if (cardToStock != null)
                    {
                        stock.AddCard(cardToStock);
                        GameState.EventBus.Raise(new GameEvent(new GameEventType("CardToStock"), new { Card = cardToStock, Source = ability.SourceCard }));
                        Debug.Log($"[{cardToStock.Data.Name}] was moved from Deck to Stock by ability.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Resolution for ability is not yet implemented: {ability.AbilityText}");
            }
        }

        public void ExecuteTurn(Player turnPlayer) {
            GameState.CurrentPlayerIndex = GameState.Players.IndexOf(turnPlayer);
            TurnPhaseTree.Execute(GameState);
        }

        public int TriggerCheck(Player attacker) {
            var deck = attacker.GetZone<IDeckZone<WeissCard>>();
            var stock = attacker.GetZone<IStockZone<WeissCard>>();
            var weissAttacker = attacker as WeissPlayer;
            if (weissAttacker == null) return 0;

            var triggeredCard = this.DrawCard(attacker);
            if (triggeredCard == null) {
                return 0;
            }

            var cardData = (triggeredCard as WeissCard)?.Data as WeissCardData;
            if (cardData == null) {
                stock.AddCard(triggeredCard);
                return 0;
            }

            GameState.EventBus.Raise(new GameEvent(new GameEventType("TriggerReveal"), new { Player = attacker, Card = triggeredCard }));

            int soulBoost = 0;
            var icons = cardData.TriggerIcon?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            foreach (var icon in icons)
            {
                switch (icon)
                {
                    case "Soul":
                        soulBoost++;
                        break;
                    case "Comeback":
                        var waitingRoom = weissAttacker.GetZone<WaitingRoomZone>();
                        var hand = weissAttacker.GetZone<IHandZone<WeissCard>>();
                        var charactersInWaitingRoom = waitingRoom.Cards
                            .Where(c => (((c as WeissCard)?.Data) as WeissCardData)?.CardType == "キャラクター")
                            .ToList();

                        var characterToReturn = weissAttacker.Controller.ChooseCardFromWaitingRoom(weissAttacker, charactersInWaitingRoom, true);
                        if (characterToReturn != null) {
                            waitingRoom.RemoveCard(characterToReturn);
                            hand.AddCard(characterToReturn);
                            GameState.EventBus.Raise(new GameEvent(new GameEventType("TriggerComeback"), new { Player = attacker, ReturnedCard = characterToReturn }));
                        }
                        break;
                    case "Draw":
                        var handZone = attacker.GetZone<IHandZone<WeissCard>>();
                        var drawnCard = deck.DrawTop();
                        if (drawnCard != null) {
                            handZone.AddCard(drawnCard);
                            GameState.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = attacker, Card = drawnCard }));
                        }
                        break;
                }
            }

            stock.AddCard(triggeredCard);
            
            if(soulBoost > 0) Debug.Log($"Triggered {soulBoost} SOUL!");

            return soulBoost;
        }

        public void ApplyDamage(Player victim, int amount) {
            var clock = victim.GetZone<IClockZone<WeissCard>>();
            var waitingRoom = victim.GetZone<WaitingRoomZone>();
            if (waitingRoom == null) {
                Debug.LogError("WaitingRoomZoneが見つかりません。");
                return;
            }

            for (int i=0;i<amount;i++) {
                var card = this.DrawCard(victim);
                if (card == null) {
                    return;
                }

                var weissCardData = (card as WeissCard)?.Data as WeissCardData;
                if (weissCardData != null && weissCardData.CardType == WeissCardType.Climax.ToString()) {
                    Debug.Log($"ダメージがキャンセルされました！ トリガーしたカード: {weissCardData.Name}");
                    waitingRoom.AddCard(card);
                    GameState.EventBus.Raise(new GameEvent(new GameEventType("DamageCancelled"), new { Player = victim, Card = card }));
                    break; 
                }

                clock.AddCard(card);
                GameState.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = victim, Card = card }));
                
                CheckAndHandleLevelUp(victim);
            }
        }

        private void CheckAndHandleLevelUp(Player player) {
            var clock = player.GetZone<ClockZone>();
            var level = player.GetZone<LevelZone>();
            var waitingRoom = player.GetZone<WaitingRoomZone>();

            while (clock.Cards.Count >= 7)
            {
                var cardsForLevelUp = clock.Cards.Take(7).ToList();

                var chosen = (player as WeissPlayer).Controller.ChooseLevelUpCard(player as WeissPlayer, cardsForLevelUp);

                level.AddCard(chosen);
                GameState.EventBus.Raise(new GameEvent(new GameEventType("LevelUp"), new { Player = player, Card = chosen }));

                clock.RemoveCards(cardsForLevelUp);
                foreach (var c in cardsForLevelUp.Where(card => card != chosen)) {
                    waitingRoom.AddCard(c);
                }
            }
        }

        private void ApplyRefreshPointDamage(Player player)
        {
            var deck = player.GetZone<IDeckZone<WeissCard>>();
            var clock = player.GetZone<IClockZone<WeissCard>>();

            var damageCard = deck.DrawTop();
            if (damageCard == null)
            {
                Debug.LogError($"Deck became empty while applying refresh point damage. This is a critical error.");
                GameState.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
                return;
            }

            clock.AddCard(damageCard);
            GameState.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = player, Card = damageCard, IsRefreshPoint = true }));
            Debug.Log($"[{damageCard.Data.Name}] was placed in clock for refresh point damage.");

            CheckAndHandleLevelUp(player);
        }

        public WeissCard DrawCard(Player player)
        {
            var deck = player.GetZone<IDeckZone<WeissCard>>();
            var card = deck.DrawTop();

            if (card == null)
            {
                Debug.Log($"{player.Name}'s deck is empty. Performing refresh.");

                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
                if (waitingRoom.Cards.Count == 0)
                {
                    GameState.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
                    Debug.LogWarning($"{player.Name} has no cards in deck or waiting room to refresh. Player loses.");
                    return null;
                }

                var cardsToReturn = new List<WeissCard>(waitingRoom.Cards);
                foreach(var c in cardsToReturn)
                {
                    waitingRoom.RemoveCard(c);
                    (deck as DeckZone)?.AddCard(c);
                }

                deck.Shuffle();
                GameState.EventBus.Raise(new GameEvent(new GameEventType("DeckRefresh"), player));
                Debug.Log($"{player.Name} refreshed their deck with {cardsToReturn.Count} cards.");

                ApplyRefreshPointDamage(player);

                card = deck.DrawTop();
                if (card == null)
                {
                    GameState.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
                    Debug.LogError($"{player.Name}'s deck is still empty after refresh. This should not happen.");
                    return null;
                }
            }

            return card;
        }

        public void ActivateAbility(WeissCard card, string abilityText)
        {
            Debug.Log($"Activating ability for [{card.Data.CardCode} {card.Data.Name}]: {abilityText}");

            const string targetAbilityText = "【起】 あなたは自分のキャラを1枚選び、【レスト】する。そうしたら、あなたは1枚引く。";

            if (abilityText == targetAbilityText)
            {
                var player = card.Owner as WeissPlayer;
                if (player == null) return;

                bool costPaid = true; 
                if (costPaid)
                {
                    var stage = player.GetZone<IStageZone<WeissCard>>();
                    var validTargets = stage.Cards.Where(c => c != null && !c.IsRested).ToList();
                    string prompt = "Choose 1 of your characters to rest.";

                    var targetToRest = player.Controller.ChooseTargetCard(player, validTargets, prompt, false);

                    if (targetToRest != null)
                    {
                        targetToRest.Rest();
                        Debug.Log($"[{targetToRest.Data.Name}] was rested by ability effect.");

                        var hand = player.GetZone<IHandZone<WeissCard>>();
                        var drawnCard = this.DrawCard(player);
                        if (drawnCard != null)
                        {
                            hand.AddCard(drawnCard);
                            GameState.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = player, Card = drawnCard }));
                            Debug.Log($"{player.Name} drew a card due to ability effect.");
                        }
                        
                        GameState.EventBus.Raise(new GameEvent(new GameEventType("AbilityResolved"), new { Player = card.Owner, Card = card, Effect = abilityText }));
                    }
                    else
                    {
                        Debug.Log("No target chosen. Ability effect fails.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Unknown or not-yet-implemented ability: {abilityText}");
            }
        }

        public void ResolveCounterAbility(WeissCard counterCard, WeissCard defendingCharacter)
        {
            if (counterCard == null || defendingCharacter == null) return;

            string assistText = null;
            if (counterCard.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
            {
                if (abilitiesObj is List<string> abilities)
                {
                    assistText = abilities.FirstOrDefault(text => text.StartsWith("【助太刀"));
                }
            }

            if (assistText == null)
            {
                Debug.LogError($"Could not find assist ability on counter card: {counterCard.Data.Name}");
                return;
            }

            Debug.Log($"Resolving counter ability: {assistText}");

            var match = System.Text.RegularExpressions.Regex.Match(assistText, @"【助太刀(?<power>\d+)\s+レベル(?<level>\d+)】");
            if (match.Success && int.TryParse(match.Groups["power"].Value, out int powerBoost) && int.TryParse(match.Groups["level"].Value, out int levelReq))
            {
                var player = defendingCharacter.Owner as WeissPlayer;
                if (player.GetZone<LevelZone>().Count < levelReq)
                {
                    Debug.Log($"Counter card level requirement not met. Player level: {player.GetZone<LevelZone>().Count}, required: {levelReq}");
                    return; 
                }

                if (!PayCounterCost(counterCard))
                {
                    Debug.Log("Could not pay counter cost.");
                    return;
                }

                defendingCharacter.TemporaryPower += powerBoost;
                GameState.EventBus.Raise(new GameEvent(new GameEventType("PowerBoosted"), new {
                    Target = defendingCharacter,
                    Amount = powerBoost,
                    Source = counterCard
                }));
                Debug.Log($"[{defendingCharacter.Data.Name}] power boosted by {powerBoost}. New temporary power: {defendingCharacter.TemporaryPower}");
            }
            else
            {
                Debug.LogError($"Could not parse power and level from assist ability: {assistText}");
            }
        }

        private bool PayCounterCost(WeissCard counterCard)
        {
            var player = counterCard.Owner as WeissPlayer;
            var stock = player.GetZone<IStockZone<WeissCard>>();
            var hand = player.GetZone<IHandZone<WeissCard>>();
            var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();

            if (stock.Cards.Count < 1)
            {
                Debug.Log("Not enough stock to pay for counter.");
                return false;
            }
            var stockCard = stock.RemoveTopCard();
            if (stockCard != null) waitingRoom.AddCard(stockCard);

            hand.RemoveCard(counterCard);
            waitingRoom.AddCard(counterCard);

            Debug.Log($"Paid cost for counter: 1 stock and discarded [{counterCard.Data.Name}]");
            GameState.EventBus.Raise(new GameEvent(new GameEventType("CounterCardPlayed"), new { Player = player, Card = counterCard }));
            return true;
        }
    }
}