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

    // メインフェイズ（詳細は後回し）
    public class MainPhase : PhaseBase {
        public MainPhase() : base("turn.main", "Main Phase") {}
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfMainPhase"));
            // TODO: メインフェイズのアクションループを実装する。プレイヤーが「フェイズ終了」を選択するまで、キャラのプレイ、イベントの使用、能力の起動などを実行できるようにする。
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

    /// <summary>
    /// アタックフェイズを管理するクラス。
    /// アタック宣言からバトル解決までの一連のサブフェイズのループを制御します。
    /// </summary>
    public class AttackPhase : PhaseBase
    {
        public AttackPhase() : base("turn.attack", "Attack Phase")
        {
            // アタックのサブフェイズを定義
            AddSubPhase(new SimplePhase("turn.attack.declare", "Attack Declaration Step"));
            AddSubPhase(new SimplePhase("turn.attack.trigger", "Trigger Step"));
            AddSubPhase(new SimplePhase("turn.attack.counter", "Counter Step"));
            AddSubPhase(new SimplePhase("turn.attack.damage", "Damage Step"));
            AddSubPhase(new SimplePhase("turn.attack.battle", "Battle Step"));
        }

        public override void OnEnter(GameState state)
        {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfAttackPhase"));

            // アタック可能なキャラがいなくなるか、プレイヤーがアタック終了を選択するまでループ
            while (true)
            {
                // 7.2.1.3 アタック可能なキャラをリストアップ
                var stage = player.GetZone<StageZone>();
                var attackableCharacters = stage.Slots
                    .Where(s => s.Current != null && s.Current.IsRested == false && stage.IsFrontRow(s as StageSlot))
                    .Select(s => s.Current)
                    .ToList();

                // 7.2.1.3.1.3 アタック可能なキャラがいない場合
                if (!attackableCharacters.Any())
                {
                    break; // ループを抜けてアンコールステップへ
                }

                // TODO: [PlayerController] プレイヤーにアタックするか、アタックを終了するか選択させる
                // 仮実装: 常にアタックを選択
                bool endAttack = false; // player.Controller.ChooseToEndAttack();
                if (endAttack)
                {
                    break;
                }

                // TODO: [PlayerController] プレイヤーに攻撃するキャラを選択させる
                // 仮実装: 攻撃可能なキャラの最初の1枚を選択
                var attacker = attackableCharacters[0];

                // 7.2.1.4 攻撃方法の選択
                var opponent = state.Players.First(p => p != player);
                var attackerSlot = stage.FindSlot(attacker);
                var defendingSlot = stage.GetOpposingSlot(attackerSlot, opponent.GetZone<StageZone>());
                var defender = defendingSlot?.Current;

                AttackType attackType;
                if (defender == null)
                {
                    // 7.2.1.4.1 ダイレクトアタック
                    attackType = AttackType.Direct;
                    // TODO: [ルール実装] アタッカーのソウルを+1する
                }
                else
                {
                    // TODO: [PlayerController] プレイヤーにフロントアタックかサイドアタックかを選択させる
                    // 仮実装: 常にフロントアタック
                    attackType = AttackType.Front;
                }

                // 7.2.1.5 アタック状態の記録とキャラのレスト
                // TODO: アタック状態を管理するクラスを作成して記録
                attacker.Rest();
                state.EventBus.Raise(new GameEvent(WeissGameEvents.AttackDeclared, new { Attacker = attacker, Defender = defender, Type = attackType }));

                // 7.2.1.6 チェックタイミング -> サブフェイズ実行
                ExecuteSubPhases(state, attackType);

                // TODO: [ルール実装] 先攻第1ターンのアタックは1回のみ、というルールを実装する
            }

            // 7.7 アンコールステップへ
            // TODO: [フェーズ実装] EncoreStepのフェーズを作成し、そこに遷移するロジックを実装する
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEncoreStep"));
        }

        private void ExecuteSubPhases(GameState state, AttackType attackType)
        {
            // 7.3 トリガーステップ
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfTriggerStep"));
            var ruleEngine = (state.Game as WeissGame).RuleEngine;
            ruleEngine.TriggerCheck(state.ActivePlayer);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfTriggerStep"));

            if (attackType == AttackType.Front)
            {
                // 7.4 カウンターステップ
                state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfCounterStep"));
                // TODO: [ルール実装] 非ターンプレイヤーにカウンターカードの使用タイミングを与える
                state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfCounterStep"));
            }

            // 7.5 ダメージステップ
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDamageStep"));
            // TODO: [ルール実装] ダメージ計算（アタッカーのソウル値）と適用（RuleEngine.ApplyDamage呼び出し）を実装する
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfDamageStep"));

            if (attackType == AttackType.Front)
            {
                // 7.6 バトルステップ
                state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfBattleStep"));
                // TODO: [ルール実装] アタッカーとディフェンダーのパワーを比較し、低い方をリバース状態にする
                state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfBattleStep"));
            }
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