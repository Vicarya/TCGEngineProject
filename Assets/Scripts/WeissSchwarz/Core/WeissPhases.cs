using TCG.Core;
using UnityEngine;
using System;
using TCG.Weiss;
using System.Linq;
using System.Collections.Generic;

namespace TCG.Weiss {
    public static class WeissPhaseFactory {
        // フェーズID は文字列で表現（入れ子の prefix ルールなどで柔軟に扱える）
        public static PhaseBase CreateTurnPhaseTree() {
            var turn = new SimplePhase("turn", "Turn");

            var stand = new StandPhase();
            var draw = new DrawPhase();
            var clock = new ClockPhase();
            var main = new MainPhase();
            var climax = new ClimaxPhase();
            // AttackPhaseは前回実装したものを想定
            var attack = new AttackPhase(); 
            var end = new EndPhase();

            turn.AddSubPhase(stand);
            turn.AddSubPhase(draw);
            turn.AddSubPhase(clock);
            turn.AddSubPhase(main);
            turn.AddSubPhase(climax);
            turn.AddSubPhase(attack);
            turn.AddSubPhase(end);

            return turn;
        }
    }

    
    // スタンドフェイズ
    public class StandPhase : PhaseBase {
        public StandPhase() : base("turn.stand", "Stand Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            state.TurnCounter++;
            var player = state.ActivePlayer as WeissPlayer;

            // Reset controller state for the new turn
            player?.Controller.ResetTurnState();

            // 舞台のキャラを全てスタンド
            foreach (var slot in player.GetZone<IStageZone<WeissCard>>().Slots) {
                if(slot.Current != null) slot.Current.SetRested(false);
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("AllCharactersStood"), player));
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterStand"));
        }
    }

    // ドローフェイズ
    public class DrawPhase : PhaseBase {
        public DrawPhase() : base("turn.draw", "Draw Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDrawPhase"));
            // 山札から1枚ドロー
            var ruleEngine = (state.Game as WeissGame).RuleEngine;
            var hand = player.GetZone<IHandZone<WeissCard>>();
            var card = ruleEngine.DrawCard(player);
            if (card != null) hand.AddCard(card);

            state.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = player, Card = card }));
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterDraw"));
        }
    }

    // クロックフェイズ
    public class ClockPhase : PhaseBase {
        public ClockPhase() : base("turn.clock", "Clock Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClockPhase"));
            // 任意: 手札1枚をクロックに置ける
            var chosen = player.Controller.ChooseCardFromHand(player, optional: true);
            if (chosen != null) {
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(chosen);
                player.GetZone<IClockZone<WeissCard>>().AddCard(chosen);
                // 2ドロー
                var ruleEngine = (state.Game as WeissGame).RuleEngine;
                for (int i = 0; i < 2; i++) {
                    var c = ruleEngine.DrawCard(player);
                    if (c != null) player.GetZone<IHandZone<WeissCard>>().AddCard(c);
                }
                state.EventBus.Raise(new GameEvent(new GameEventType("CardClocked"), new { Player = player, Card = chosen }));
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterClock"));
        }
    }

    // メインフェイズ
    public class MainPhase : PhaseBase {
        public MainPhase() : base("turn.main", "Main Phase") {}
        public override void OnEnter(GameState state)
        {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfMainPhase"));

            while (true)
            {
                var action = player.Controller.ChooseMainPhaseAction(player);

                if (action == MainPhaseAction.EndPhase)
                {
                    break;
                }
                else if (action == MainPhaseAction.PlayCard)
                {
                    var cardToPlay = player.Controller.ChooseCardFromHand(player, optional: true);
                    if (cardToPlay == null) continue;

                    var data = cardToPlay.Data as WeissCardData;
                    var playerLevel = player.GetZone<LevelZone>().Count;
                    var stockZone = player.GetZone<IStockZone<WeissCard>>();

                    // 1. レベルチェック
                    if (data.Level > playerLevel)
                    {
                        Debug.Log($"プレイするにはレベルが足りません。プレイヤーレベル: {playerLevel}, カードレベル: {data.Level}");
                        continue;
                    }

                    // 2. コストチェック
                    if (stockZone.Cards.Count < data.Cost)
                    {
                        Debug.Log($"プレイするにはストックが足りません。必要ストック: {data.Cost}, 現在のストック: {stockZone.Cards.Count}");
                        continue;
                    }

                    // 3. カード種別ごとの処理
                    if (data.CardType == WeissCardType.Character.ToString())
                    {
                        var stageZone = player.GetZone<IStageZone<WeissCard>>();
                        var emptySlot = stageZone.Slots.FirstOrDefault(s => s.Current == null);
                        if (emptySlot == null)
                        {
                            Debug.Log("舞台に空きがありません。");
                            continue;
                        }
                        
                        // 全てのチェックをクリアしたので、コストを支払い、カードをプレイする
                        PayCost(player, data.Cost);
                        player.GetZone<IHandZone<WeissCard>>().RemoveCard(cardToPlay);
                        emptySlot.PlaceCharacter(cardToPlay);
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.CardPlayed, new { Player = player, Card = cardToPlay }));
                    }
                    else if (data.CardType == WeissCardType.Event.ToString())
                    {
                        // 全てのチェックをクリアしたので、コストを支払い、カードをプレイする
                        PayCost(player, data.Cost);
                        player.GetZone<IHandZone<WeissCard>>().RemoveCard(cardToPlay);
                        player.GetZone<ResolutionZone>().AddCard(cardToPlay);
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.CardPlayed, new { Player = player, Card = cardToPlay }));
                        // TODO: イベントカードの効果を解決する
                    }
                    else
                    {
                        Debug.Log($"このカードタイプ({data.CardType})はメインフェイズにプレイできません。");
                    }
                }
                else if (action == MainPhaseAction.UseAbility)
                {
                    // 1. Find all activatable abilities
                    var stageZone = player.GetZone<IStageZone<WeissCard>>();
                    var activatableAbilities = new List<KeyValuePair<WeissCard, string>>();

                    foreach (var slot in stageZone.Slots)
                    {
                        var card = slot.Current;
                        if (card != null && !card.IsRested && card.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
                        {
                            if (abilitiesObj is List<string> abilities)
                            {
                                foreach (var abilityText in abilities)
                                {
                                    if (abilityText.StartsWith("【起】"))
                                    {
                                        activatableAbilities.Add(new KeyValuePair<WeissCard, string>(card, abilityText));
                                    }
                                }
                            }
                        }
                    }

                    // 2. Have the player choose an ability
                    if (activatableAbilities.Any())
                    {
                        var chosenAbility = player.Controller.ChooseAbilityToActivate(player, activatableAbilities);

                        // 3. Activate the chosen ability via the rule engine
                        if (chosenAbility.Key != null)
                        {
                            var ruleEngine = (state.Game as WeissGame).RuleEngine;
                            ruleEngine.ActivateAbility(chosenAbility.Key, chosenAbility.Value);
                        }
                    }
                    else
                    {
                        Debug.Log("There are no activatable abilities.");
                    }
                }
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterMainActions"));
        }

        private void PayCost(WeissPlayer player, int stockCost)
        {
            if (stockCost <= 0) return;

            var stockZone = player.GetZone<IStockZone<WeissCard>>();
            var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();

            for (int i = 0; i < stockCost; i++)
            {
                var card = stockZone.RemoveTopCard();
                if(card != null) waitingRoom.AddCard(card);
            }
        }
    }

    // クライマックスフェイズ
    public class ClimaxPhase : PhaseBase {
        public ClimaxPhase() : base("turn.climax", "Climax Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClimaxPhase"));
            // 任意: クライマックスカードを1枚だけ置ける
            var chosen = player.Controller.ChooseClimaxFromHand(player, optional: true);
            if (chosen != null) {
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(chosen);
                player.GetZone<IClimaxZone<WeissCard>>().AddCard(chosen);
                state.EventBus.Raise(new GameEvent(WeissGameEvents.ClimaxPlayed, new { Player = player, Card = chosen }));
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterClimax"));
        }
    }

    public class AttackPhase : PhaseBase
    {
        public AttackPhase() : base("turn.attack", "Attack Phase") { }

        public override void OnEnter(GameState state)
        {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;
            var ruleEngine = (state.Game as WeissGame).RuleEngine;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfAttackPhase"));

            bool isFirstTurn = state.TurnCounter == 1;
            int attacksThisTurn = 0;

            while (true)
            {
                var stage = player.GetZone<StageZone>();
                var attackableCharacters = stage.Slots
                    .Where(s => s.Current != null && !s.Current.IsRested && stage.IsFrontRow(s as StageSlot))
                    .Select(s => s.Current)
                    .ToList();

                if (!attackableCharacters.Any() || player.Controller.ChooseToEndAttack(player))
                {
                    break;
                }

                var attacker = player.Controller.ChooseAttacker(player, attackableCharacters);
                if (attacker == null) continue; // Should not happen with dummy controller

                var opponent = state.Players.First(p => p != player) as WeissPlayer;
                var attackerSlot = stage.FindSlot(attacker);
                var opponentStage = opponent.GetZone<StageZone>();
                var defendingSlot = stage.GetOpposingSlot(attackerSlot, opponentStage);
                var defender = defendingSlot?.Current;

                AttackType attackType;
                if (defender == null)
                {
                    attackType = AttackType.Direct;
                }
                else
                {
                    attackType = player.Controller.ChooseAttackType(player, attacker, defender);
                }

                attacker.Rest();
                state.EventBus.Raise(new GameEvent(WeissGameEvents.AttackDeclared, new { Attacker = attacker, Defender = defender, Type = attackType }));

                // アタック時自動能力の解決
                ResolveOnAttackAbilities(state, attacker);

                // Execute sub-phases
                int soulBoost = ExecuteTriggerStep(state, ruleEngine);

                if (attackType == AttackType.Front)
                {
                    ExecuteCounterStep(state, opponent);
                }

                ExecuteDamageStep(state, ruleEngine, attacker, opponent, attackType, soulBoost);

                if (attackType == AttackType.Front && defender != null)
                {
                    ExecuteBattleStep(state, attacker, defender);
                }
                
                attacksThisTurn++;
                if (isFirstTurn && attacksThisTurn >= 1)
                {
                    Debug.Log("最初のターンは1回しかアタックできません。");
                    break;
                }
            }

            ExecuteEncoreStep(state);
        }

        private int ExecuteTriggerStep(GameState state, WeissRuleEngine ruleEngine)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfTriggerStep"));
            int soulBoost = ruleEngine.TriggerCheck(state.ActivePlayer);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfTriggerStep"));
            return soulBoost;
        }

        private void ExecuteCounterStep(GameState state, WeissPlayer opponent)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfCounterStep"));

            // 1. Find counter cards in opponent's hand
            var hand = opponent.GetZone<IHandZone<WeissCard>>();
            var counterCards = hand.Cards.Where(card => {
                if (card.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj)) {
                    if (abilitiesObj is List<string> abilities) {
                        return abilities.Any(text => text.Contains("【助太刀】"));
                    }
                }
                return false;
            }).ToList();

            // 2. Ask controller to choose one
            var chosenCounter = opponent.Controller.ChooseCounterCardFromHand(opponent, counterCards);

            // 3. If a card is chosen, process it (simplified for now)
            if (chosenCounter != null)
            {
                Debug.Log($"{opponent.Name} plays counter card: [{chosenCounter.Data.CardCode}] {chosenCounter.Data.Name}");
                // TODO: Implement cost payment and effect resolution via RuleEngine
                
                // For now, just move card from hand to resolution zone as a placeholder action
                hand.RemoveCard(chosenCounter);
                opponent.GetZone<ResolutionZone>().AddCard(chosenCounter);
                state.EventBus.Raise(new GameEvent(new GameEventType("CounterCardPlayed"), new { Player = opponent, Card = chosenCounter }));
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfCounterStep"));
        }

        private void ExecuteDamageStep(GameState state, WeissRuleEngine ruleEngine, WeissCard attacker, WeissPlayer opponent, AttackType attackType, int soulBoost)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDamageStep"));
            var soul = (attacker.Data as WeissCardData).Soul + soulBoost;
            if (attackType == AttackType.Direct)
            {
                soul += 1;
            }
            ruleEngine.ApplyDamage(opponent, soul);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfDamageStep"));
        }

        private void ExecuteBattleStep(GameState state, WeissCard attacker, WeissCard defender)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfBattleStep"));
            var attackerPower = (attacker.Data as WeissCardData).Power;
            var defenderPower = (defender.Data as WeissCardData).Power;

            if (attackerPower < defenderPower)
            {
                attacker.SetReversed(true);
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, attacker));
            }
            else if (defenderPower < attackerPower)
            {
                defender.SetReversed(true);
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, defender));
            }
            else // Same power
            {
                attacker.SetReversed(true);
                defender.SetReversed(true);
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, attacker));
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, defender));
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfBattleStep"));
        }

        private void ExecuteEncoreStep(GameState state)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEncoreStep"));
            Debug.Log("Start of Encore Step");

            // The turn player gets to decide the order of encores first.
            // For simplicity, we will process the turn player's reversed cards, then the other player's.
            var players = new List<WeissPlayer> { state.ActivePlayer as WeissPlayer, state.Players.FirstOrDefault(p => p != state.ActivePlayer) as WeissPlayer };

            foreach (var player in players)
            {
                if (player == null) continue;

                var stage = player.GetZone<StageZone>();
                var waitingRoom = player.GetZone<WaitingRoomZone>();
                
                // Find reversed characters for the current player
                var reversedCharacters = stage.Slots
                    .Where(s => s.Current != null && s.Current.IsReversed)
                    .Select(s => s.Current)
                    .ToList();

                foreach (var character in reversedCharacters)
                {
                    var originalSlot = stage.FindSlot(character);

                    // 1. Move the character to the waiting room
                    originalSlot.RemoveCharacter();
                    waitingRoom.AddCard(character);
                    character.SetReversed(false); // No longer reversed in the waiting room
                    state.EventBus.Raise(new GameEvent(new GameEventType("CharacterToWaitingRoom"), new { Character = character, From = "Stage" }));
                    Debug.Log($"[{character.Data.Name}] was sent from Stage to Waiting Room.");

                    // 2. Ask player if they want to encore
                    var choice = player.Controller.ChooseToEncore(player, character);

                    bool encored = false;
                    if (choice == EncoreChoice.Standard)
                    {
                        // 3a. Standard Encore
                        var stock = player.GetZone<IStockZone<WeissCard>>();
                        if (stock.Cards.Count >= 3)
                        {
                            // Pay cost
                            for(int i = 0; i < 3; i++)
                            {
                                var card = stock.RemoveTopCard();
                                if(card != null) waitingRoom.AddCard(card);
                            }
                            
                            // Encore the character
                            waitingRoom.RemoveCard(character);
                            originalSlot.PlaceCharacter(character);
                            character.Rest();
                            encored = true;
                            Debug.Log($"[{character.Data.Name}] was encored by paying 3 stock.");
                            state.EventBus.Raise(new GameEvent(new GameEventType("CharacterEncored"), new { Character = character, Type = "Standard" }));
                        }
                    }
                    else if (choice == EncoreChoice.Special)
                    {
                        // 3b. Special Encore
                        // For now, we only handle "discard a character from hand"
                        // A more robust implementation would parse the cost from the ability text.
                        var hand = player.GetZone<IHandZone<WeissCard>>();
                        var characterInHand = hand.Cards.FirstOrDefault(c => ((c as WeissCard)?.Data as WeissCardData)?.CardType == "キャラクター");

                        if (characterInHand != null)
                        {
                            // Pay cost
                            hand.RemoveCard(characterInHand);
                            waitingRoom.AddCard(characterInHand);
                            
                            // Encore the character
                            waitingRoom.RemoveCard(character);
                            originalSlot.PlaceCharacter(character);
                            character.Rest();
                            encored = true;
                            Debug.Log($"[{character.Data.Name}] was encored by discarding [{characterInHand.Data.Name}].");
                            state.EventBus.Raise(new GameEvent(new GameEventType("CharacterEncored"), new { Character = character, Type = "Special", CostCard = characterInHand }));
                        }
                        else {
                            Debug.Log($"Could not pay special encore cost for [{character.Data.Name}] (no character in hand to discard).");
                        }
                    }

                    if (!encored)
                    {
                        Debug.Log($"[{character.Data.Name}] was not encored.");
                    }
                }
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfEncoreStep"));
            Debug.Log("End of Encore Step");
        }

        private void ResolveOnAttackAbilities(GameState state, WeissCard attacker)
        {
            var player = attacker.Owner as WeissPlayer;
            if (player == null) return;

            // Proof-of-concept: Check for a specific hardcoded auto ability
            const string onAttackAbilityText = "【自】 このカードがアタックした時、あなたは自分の山札の上から1枚を、ストック置場に置いてよい。";

            if (attacker.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj))
            {
                if (abilitiesObj is List<string> abilities && abilities.Contains(onAttackAbilityText))
                {
                    // Found the ability, ask the player if they want to use it.
                    bool useAbility = player.Controller.AskYesNo(player, $"Use ability? \"{onAttackAbilityText}\"");

                    if (useAbility)
                    {
                        Debug.Log($"Player chose to use on-attack ability for [{attacker.Data.Name}].");
                        // Resolve the effect: top card of deck to stock.
                        var deck = player.GetZone<IDeckZone<WeissCard>>();
                        var stock = player.GetZone<IStockZone<WeissCard>>();
                        var ruleEngine = (state.Game as WeissGame).RuleEngine;

                        var cardToStock = ruleEngine.DrawCard(player);
                        if (cardToStock != null)
                        {
                            stock.AddCard(cardToStock);
                            state.EventBus.Raise(new GameEvent(new GameEventType("CardToStock"), new { Card = cardToStock, Source = attacker }));
                            Debug.Log($"[{cardToStock.Data.Name}] was moved from Deck to Stock.");
                        }
                    }
                }
            }
        }
    }

    // エンドフェイズ
    public class EndPhase : PhaseBase {
        public EndPhase() : base("turn.end", "End Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEndPhase"));
            // 手札上限調整
            while (player.GetZone<IHandZone<WeissCard>>().Cards.Count > player.HandLimit) {
                var discard = player.Controller.ChooseCardFromHand(player, optional: false);
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(discard);
                player.GetZone<IDiscardPile<WeissCard>>().AddCard(discard);
                state.EventBus.Raise(new GameEvent(new GameEventType("DiscardForHandLimit"), discard));
            }
            // クライマックス置場を片付ける
            var climax = player.GetZone<IClimaxZone<WeissCard>>();
            foreach (var card in climax.Cards.ToArray()) {
                climax.RemoveCard(card);
                player.GetZone<IDiscardPile<WeissCard>>().AddCard(card);
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("ClimaxCleared"), player));
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterEnd"));
        }
    }
}