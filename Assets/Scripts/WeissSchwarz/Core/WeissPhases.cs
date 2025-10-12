using TCG.Core;
using System;
using TCG.Weiss;
using System.Linq;

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
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfStandPhase"));
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
            var deck = player.GetZone<IDeckZone<WeissCard>>();
            var hand = player.GetZone<IHandZone<WeissCard>>();
            var card = deck.DrawTop();
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
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClockPhase"));
            // 任意: 手札1枚をクロックに置ける
            var chosen = ((WeissPlayer)player).Controller.ChooseCardFromHand(player, optional: true);
            if (chosen != null) {
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(chosen);
                player.GetZone<IClockZone<WeissCard>>().AddCard(chosen);
                // 2ドロー
                for (int i = 0; i < 2; i++) {
                    var c = player.GetZone<IDeckZone<WeissCard>>().DrawTop();
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
        public override void OnEnter(GameState state) {
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
                    if (cardToPlay != null)
                    {
                        // TODO: Implement card playing logic (move from hand to stage/resolution zone, pay costs, etc.)
                        player.GetZone<IHandZone<WeissCard>>().RemoveCard(cardToPlay);
                        // This is a simplification. Real logic would depend on card type.
                        player.GetZone<IStageZone<WeissCard>>().Slots.First(s => s.Current == null)?.PlaceCharacter(cardToPlay);
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.CardPlayed, new { Player = player, Card = cardToPlay }));
                    }
                }
                else if (action == MainPhaseAction.UseAbility)
                {
                    // TODO: Implement ability usage logic
                }
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterMainActions"));
        }
    }

    // クライマックスフェイズ
    public class ClimaxPhase : PhaseBase {
        public ClimaxPhase() : base("turn.climax", "Climax Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClimaxPhase"));
            // 任意: クライマックスカードを1枚だけ置ける
            var chosen = ((WeissPlayer)player).Controller.ChooseClimaxFromHand(player, optional: true);
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

                // Execute sub-phases
                ExecuteTriggerStep(state, ruleEngine);

                if (attackType == AttackType.Front)
                {
                    ExecuteCounterStep(state);
                }

                ExecuteDamageStep(state, ruleEngine, attacker, opponent, attackType);

                if (attackType == AttackType.Front && defender != null)
                {
                    ExecuteBattleStep(state, attacker, defender);
                }
                
                // TODO: Implement first turn, one attack rule
            }

            // TODO: Implement EncoreStep
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEncoreStep"));
        }

        private void ExecuteTriggerStep(GameState state, WeissRuleEngine ruleEngine)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfTriggerStep"));
            ruleEngine.TriggerCheck(state.ActivePlayer);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfTriggerStep"));
        }

        private void ExecuteCounterStep(GameState state)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfCounterStep"));
            // TODO: Allow non-active player to play counter cards
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfCounterStep"));
        }

        private void ExecuteDamageStep(GameState state, WeissRuleEngine ruleEngine, WeissCard attacker, WeissPlayer opponent, AttackType attackType)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDamageStep"));
            var soul = (attacker.Data as WeissCardData).Soul;
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
    }

    // エンドフェイズ
    public class EndPhase : PhaseBase {
        public EndPhase() : base("turn.end", "End Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEndPhase"));
            // 手札上限調整
            while (player.GetZone<IHandZone<WeissCard>>().Cards.Count > ((WeissPlayer)player).HandLimit) {
                var discard = ((WeissPlayer)player).Controller.ChooseCardFromHand(player, optional: false);
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