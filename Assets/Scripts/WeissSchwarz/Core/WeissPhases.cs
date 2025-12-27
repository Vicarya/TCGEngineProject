using TCG.Core;
using UnityEngine;
using System;
using TCG.Weiss;
using System.Linq;
using System.Collections.Generic;

namespace TCG.Weiss {
    /// <summary>
    /// ヴァイスシュヴァルツの1ターンのフェーズ構成を生成するファクトリクラス。
    /// </summary>
    public static class WeissPhaseFactory {
        /// <summary>
        /// ヴァイスシュヴァルツのターンを構成する全てのフェーズを構築し、親子関係を設定して返します。
        /// </summary>
        /// <returns>全てのサブフェーズが設定された、ルートとなるターンフェーズ。</returns>
        public static PhaseBase CreateTurnPhaseTree() {
            var turn = new SimplePhase("turn", "Turn");

            // ターンを構成する各フェーズをインスタンス化
            var stand = new StandPhase();
            var draw = new DrawPhase();
            var clock = new ClockPhase();
            var main = new MainPhase();
            var climax = new ClimaxPhase();
            var attack = new AttackPhase(); 
            var end = new EndPhase();

            // ターンフェーズにサブフェーズとして追加していく
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

    
    /// <summary>
    /// スタンドフェイズ。ターン開始時に、レストしているカードをスタンドさせます。
    /// </summary>
    public class StandPhase : PhaseBase {
        public StandPhase() : base("turn.stand", "Stand Phase") {}
        /// <summary>
        /// フェーズ開始時、アクティブプレイヤーの場のキャラを全てスタンドさせます。
        /// </summary>
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            state.TurnCounter++;
            var player = state.ActivePlayer as WeissPlayer;

            // コントローラーのターン情報をリセット
            player?.Controller.ResetTurnState();

            // 舞台のキャラを全てスタンド状態にする
            foreach (var slot in player.GetZone<IStageZone<WeissCard>>().Slots) {
                if(slot.Current != null) slot.Current.SetRested(false);
            }

            // イベント発行: 全キャラがスタンドした
            state.EventBus.Raise(new GameEvent(new GameEventType("AllCharactersStood"), player));
            // イベント発行: 「スタンドフェイズの始め」のチェックタイミング
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterStand"));
        }
    }

    /// <summary>
    /// ドローフェイズ。山札からカードを1枚引きます。
    /// </summary>
    public class DrawPhase : PhaseBase {
        public DrawPhase() : base("turn.draw", "Draw Phase") {}
        /// <summary>
        /// フェーズ開始時、山札からカードを1枚引きます。
        /// </summary>
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer;

            // イベント発行: 「ドローフェイズの始め」のチェックタイミング
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDrawPhase"));
            
            // 山札から1枚ドローする
            var ruleEngine = (state.Game as WeissGame).RuleEngine;
            var hand = player.GetZone<IHandZone<WeissCard>>();
            var card = ruleEngine.DrawCard(player);
            if (card != null) hand.AddCard(card);

            // イベント発行: カードがドローされた
            state.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = player, Card = card }));
            // イベント発行: 「ドローフェイズの終わり」のチェックタイミング
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterDraw"));
        }
    }

    /// <summary>
    /// クロックフェイズ。手札1枚をクロックに置くことで、2枚ドローできます。
    /// </summary>
    public class ClockPhase : PhaseBase {
        public ClockPhase() : base("turn.clock", "Clock Phase") {}
        /// <summary>
        /// フェーズ開始時、プレイヤーにクロックアクション（手札をクロックに置いて2ドロー）の選択を促します。
        /// </summary>
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClockPhase"));
            
            // プレイヤーに、手札1枚をクロックに置くか選択させる（任意）
            var chosen = player.Controller.ChooseCardFromHand(player, optional: true);
            if (chosen != null) {
                // 手札からクロックへカードを移動
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(chosen);
                player.GetZone<IClockZone<WeissCard>>().AddCard(chosen);
                
                // 成功した場合、2枚ドローする
                var ruleEngine = (state.Game as WeissGame).RuleEngine;
                for (int i = 0; i < 2; i++) {
                    var c = ruleEngine.DrawCard(player);
                    if (c != null) player.GetZone<IHandZone<WeissCard>>().AddCard(c);
                }
                // イベント発行: カードがクロックに置かれた
                state.EventBus.Raise(new GameEvent(new GameEventType("CardClocked"), new { Player = player, Card = chosen }));
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterClock"));
        }
    }

    /// <summary>
    /// メインフェイズ。カードのプレイや能力の起動など、プレイヤーが自由に行動できます。
    /// </summary>
    public class MainPhase : PhaseBase {
        public MainPhase() : base("turn.main", "Main Phase") {}

        /// <summary>
        /// フェーズ開始時、プレイヤーが「フェイズ終了」を選択するまで、行動の選択をループで促します。
        /// </summary>
        public override void OnEnter(GameState state)
        {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfMainPhase"));

            // プレイヤーがメインフェイズを終了するまでループ
            while (true)
            {
                var action = player.Controller.ChooseMainPhaseAction(player);

                if (action == MainPhaseAction.EndPhase)
                {
                    break; // ループを抜けてフェイズ終了
                }
                else if (action == MainPhaseAction.PlayCard)
                {
                    // === カードをプレイする処理 ===
                    var cardToPlay = player.Controller.ChooseCardFromHand(player, optional: true);
                    if (cardToPlay == null) continue; // プレイしない場合はループの先頭へ

                    var data = cardToPlay.Data as WeissCardData;
                    var playerLevel = player.GetZone<LevelZone>().Count;
                    var stockZone = player.GetZone<IStockZone<WeissCard>>();

                    // 条件チェック1: レベル
                    if (data.Level > playerLevel)
                    {
                        Debug.Log($"プレイするにはレベルが足りません。プレイヤーレベル: {playerLevel}, カードレベル: {data.Level}");
                        continue;
                    }

                    // 条件チェック2: コスト
                    if (stockZone.Cards.Count < data.Cost)
                    {
                        Debug.Log($"プレイするにはストックが足りません。必要ストック: {data.Cost}, 現在のストック: {stockZone.Cards.Count}");
                        continue;
                    }

                    // 条件チェック3: カード種別ごとのプレイ先
                    if (data.CardType == WeissCardType.Character.ToString())
                    {
                        var stageZone = player.GetZone<IStageZone<WeissCard>>();
                        var emptySlot = stageZone.Slots.FirstOrDefault(s => s.Current == null);
                        if (emptySlot == null)
                        {
                            Debug.Log("舞台に空きがありません。");
                            continue;
                        }
                        
                        // 全てのチェックをクリアしたので、コストを支払い、カードを舞台にプレイする
                        PayCost(player, data.Cost);
                        player.GetZone<IHandZone<WeissCard>>().RemoveCard(cardToPlay);
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.CardPlayed, new CardPlayedEventArgs(player, cardToPlay)));
                    }
                    else if (data.CardType == WeissCardType.Event.ToString())
                    {
                        // イベントカードのプレイ処理
                        PayCost(player, data.Cost);
                        player.GetZone<IHandZone<WeissCard>>().RemoveCard(cardToPlay);
                        player.GetZone<ResolutionZone>().AddCard(cardToPlay); // 解決ゾーンに移動
                        state.EventBus.Raise(new GameEvent(BaseGameEvents.CardPlayed, new CardPlayedEventArgs(player, cardToPlay)));
                        // TODO: イベントカードの効果を解決するロジックを呼び出す
                    }
                    else
                    {
                        Debug.Log($"このカードタイプ({data.CardType})はメインフェイズにプレイできません。");
                    }
                }
                else if (action == MainPhaseAction.UseAbility)
                {
                    // === 起動能力を使う処理 ===
                    // 1. 起動可能な能力をすべて見つける
                    var stageZone = player.GetZone<IStageZone<WeissCard>>();
                    var activatableAbilities = new List<KeyValuePair<WeissCard, string>>();

                    foreach (var slot in stageZone.Slots)
                    {
                        var card = slot.Current;
                        // レストしていなくて、"【起】"で始まる能力を持つカードを探す
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

                    // 2. プレイヤーにどの能力を起動するか選択させる
                    if (activatableAbilities.Any())
                    {
                        var chosenAbility = player.Controller.ChooseAbilityToActivate(player, activatableAbilities);

                        // 3. 選択された能力をルールエンジン経由で起動する
                        if (chosenAbility.Key != null)
                        {
                            var ruleEngine = (state.Game as WeissGame).RuleEngine;
                            ruleEngine.ActivateAbility(chosenAbility.Key, chosenAbility.Value);
                        }
                    }
                    else
                    {
                        Debug.Log("起動可能な能力はありません。");
                    }
                }
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterMainActions"));
        }

        /// <summary>
        /// ストックから指定されたコストを支払います。
        /// </summary>
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

    /// <summary>
    /// クライマックスフェイズ。手札からクライマックスカードを1枚だけプレイできます。
    /// </summary>
    public class ClimaxPhase : PhaseBase {
        public ClimaxPhase() : base("turn.climax", "Climax Phase") {}
        /// <summary>
        /// フェーズ開始時、プレイヤーにクライマックスカードをプレイするかの選択を促します。
        /// </summary>
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfClimaxPhase"));
            
            // 任意: クライマックスカードを1枚だけプレイできる
            var chosen = player.Controller.ChooseClimaxFromHand(player, optional: true);
            if (chosen != null) {
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(chosen);
                player.GetZone<IClimaxZone<WeissCard>>().AddCard(chosen);
                // イベント発行: クライマックスがプレイされた
                state.EventBus.Raise(new GameEvent(WeissGameEvents.ClimaxPlayed, new { Player = player, Card = chosen }));
            }
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "AfterClimax"));
        }
    }

    /// <summary>
    /// アタックフェイズ。キャラクターによるアタックを処理します。
    /// </summary>
    public class AttackPhase : PhaseBase
    {
        public AttackPhase() : base("turn.attack", "Attack Phase") { }

        /// <summary>
        /// フェーズ開始時、プレイヤーがアタックを終了するまで、アタックの一連の流れをループで処理します。
        /// </summary>
        public override void OnEnter(GameState state)
        {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;
            var ruleEngine = (state.Game as WeissGame).RuleEngine;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfAttackPhase"));

            bool isFirstTurn = state.TurnCounter == 1;
            int attacksThisTurn = 0;

            // アタックのループ
            while (true)
            {
                var stage = player.GetZone<StageZone>();
                // 前列にいる、レストしていないキャラがアタック可能
                var attackableCharacters = stage.Slots
                    .Where(s => s.Current != null && !s.Current.IsRested && stage.IsFrontRow(s as StageSlot))
                    .Select(s => s.Current)
                    .ToList();

                // アタック可能なキャラがいない、またはプレイヤーがアタック終了を選択した場合、ループを抜ける
                if (!attackableCharacters.Any() || player.Controller.ChooseToEndAttack(player))
                {
                    break;
                }

                // 1. アタッカー選択
                var attacker = player.Controller.ChooseAttacker(player, attackableCharacters);
                if (attacker == null) continue; 

                var opponent = state.Players.First(p => p != player) as WeissPlayer;
                var attackerSlot = stage.FindSlot(attacker);
                var opponentStage = opponent.GetZone<StageZone>();
                var defendingSlot = stage.GetOpposingSlot(attackerSlot, opponentStage);
                var defender = defendingSlot?.Current;

                // 2. アタック方法決定（ダイレクト、フロント、サイド）
                AttackType attackType;
                if (defender == null)
                {
                    attackType = AttackType.Direct; // 正面に相手キャラがいなければダイレクトアタック
                }
                else
                {
                    attackType = player.Controller.ChooseAttackType(player, attacker, defender);
                }

                // アタック宣言
                attacker.Rest();
                state.EventBus.Raise(new GameEvent(WeissGameEvents.AttackDeclared, new AttackDeclaredEventArgs(attacker, defender, attackType)));

                // --- サブフェーズの実行 ---
                // 3. トリガーステップ
                int soulBoost = ExecuteTriggerStep(state, ruleEngine);

                // 4. カウンターステップ (フロントアタック時のみ)
                if (attackType == AttackType.Front && defender != null)
                {
                    ExecuteCounterStep(state, opponent, defender);
                }

                // 5. ダメージステップ
                ExecuteDamageStep(state, ruleEngine, attacker, opponent, attackType, soulBoost);

                // 6. バトルステップ (フロントアタック時のみ)
                if (attackType == AttackType.Front && defender != null)
                {
                    ExecuteBattleStep(state, attacker, defender);
                }
                
                attacksThisTurn++;
                // ゲームの最初のターンは1回しかアタックできないルール
                if (isFirstTurn && attacksThisTurn >= 1)
                {
                    Debug.Log("最初のターンは1回しかアタックできません。");
                    break;
                }
            }

            // 7. アンコールステップ
            ExecuteEncoreStep(state);
        }

        private int ExecuteTriggerStep(GameState state, WeissRuleEngine ruleEngine)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfTriggerStep"));
            int soulBoost = ruleEngine.TriggerCheck(state.ActivePlayer);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfTriggerStep"));
            return soulBoost;
        }

        private void ExecuteCounterStep(GameState state, WeissPlayer opponent, WeissCard defendingCharacter)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfCounterStep"));

            // 1. 相手の手札にある「助太刀」能力を持つカードを探す
            var hand = opponent.GetZone<IHandZone<WeissCard>>();
            var counterCards = hand.Cards.Where(card => {
                if (card.Data.Metadata.TryGetValue("ability_text", out object abilitiesObj)) {
                    if (abilitiesObj is List<string> abilities) {
                        return abilities.Any(text => text.Contains("【助太刀】"));
                    }
                }
                return false;
            }).ToList();

            if (!counterCards.Any()) {
                state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfCounterStep"));
                return;
            }

            // 2. 相手プレイヤーに助太刀を使うか選択させる
            var chosenCounter = opponent.Controller.ChooseCounterCardFromHand(opponent, counterCards);

            // 3. 助太刀が使われたら、ルールエンジンで効果を解決
            if (chosenCounter != null)
            {
                var ruleEngine = (state.Game as WeissGame).RuleEngine;
                ruleEngine.ResolveCounterAbility(chosenCounter, defendingCharacter);
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfCounterStep"));
        }

        private void ExecuteDamageStep(GameState state, WeissRuleEngine ruleEngine, WeissCard attacker, WeissPlayer opponent, AttackType attackType, int soulBoost)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfDamageStep"));
            var soul = (attacker.Data as WeissCardData).Soul + soulBoost;
            if (attackType == AttackType.Direct)
            {
                soul += 1; // ダイレクトアタックはソウル+1
            }
            ruleEngine.ApplyDamage(opponent, soul);
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfDamageStep"));
        }

        private void ExecuteBattleStep(GameState state, WeissCard attacker, WeissCard defender)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfBattleStep"));
            var attackerPower = (attacker.Data as WeissCardData).Power + attacker.TemporaryPower;
            var defenderPower = (defender.Data as WeissCardData).Power + defender.TemporaryPower;

            Debug.Log($"バトル: [{attacker.Data.Name}] ({attackerPower} パワー) vs [{defender.Data.Name}] ({defenderPower} パワー)");

            // パワーを比較し、低い方がリバースする
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
            else // パワーが同じ場合は相打ち
            {
                attacker.SetReversed(true);
                defender.SetReversed(true);
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, attacker));
                state.EventBus.Raise(new GameEvent(WeissGameEvents.CharacterReversed, defender));
            }

            // バトル終了後、一時的なパワー修整をリセット
            attacker.TemporaryPower = 0;
            defender.TemporaryPower = 0;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfBattleStep"));
        }

        private void ExecuteEncoreStep(GameState state)
        {
            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEncoreStep"));
            Debug.Log("アンコールステップ開始");

            // ターンプレイヤーから順にアンコールの処理を行う
            var players = new List<WeissPlayer> { state.ActivePlayer as WeissPlayer, state.Players.FirstOrDefault(p => p != state.ActivePlayer) as WeissPlayer };

            foreach (var player in players)
            {
                if (player == null) continue;

                var stage = player.GetZone<StageZone>();
                var waitingRoom = player.GetZone<WaitingRoomZone>();
                
                // このプレイヤーのリバースしているキャラをすべて取得
                var reversedCharacters = stage.Slots
                    .Where(s => s.Current != null && s.Current.IsReversed)
                    .Select(s => s.Current)
                    .ToList();

                foreach (var character in reversedCharacters)
                {
                    var originalSlot = stage.FindSlot(character);

                    // 1. まずリバースしたキャラを舞台から控え室に送る
                    originalSlot.RemoveCharacter();
                    waitingRoom.AddCard(character);
                    character.SetReversed(false); // 控え室ではリバース状態は解除
                    Debug.Log($"[{character.Data.Name}] が舞台から控え室に送られました。");

                    // 2. プレイヤーにアンコールするかどうか選択させる
                    var choice = player.Controller.ChooseToEncore(player, character);

                    bool encored = false;
                    if (choice == EncoreChoice.Standard)
                    {
                        // 3a. 通常アンコール（3コスト）
                        var stock = player.GetZone<IStockZone<WeissCard>>();
                        if (stock.Cards.Count >= 3)
                        {
                            // コスト支払い
                            for(int i = 0; i < 3; i++)
                            {
                                var card = stock.RemoveTopCard();
                                if(card != null) waitingRoom.AddCard(card);
                            }
                            
                            // キャラを控え室から元の場所に戻し、レスト状態で配置
                            waitingRoom.RemoveCard(character);
                            originalSlot.PlaceCharacter(character);
                            character.Rest();
                            encored = true;
                            Debug.Log($"[{character.Data.Name}] が3コストでアンコールされました。");
                        }
                    }
                    else if (choice == EncoreChoice.Special)
                    {
                        // 3b. 特殊アンコール（例：手札のキャラ1枚を控え室に置く）
                        // 本来は能力テキストからコストを正確に解釈する必要がある
                        var hand = player.GetZone<IHandZone<WeissCard>>();
                        var characterInHand = hand.Cards.FirstOrDefault(c => ((c as WeissCard)?.Data as WeissCardData)?.CardType == "キャラクター");

                        if (characterInHand != null)
                        {
                            // コスト支払い
                            hand.RemoveCard(characterInHand);
                            waitingRoom.AddCard(characterInHand);
                            
                            // アンコール
                            waitingRoom.RemoveCard(character);
                            originalSlot.PlaceCharacter(character);
                            character.Rest();
                            encored = true;
                            Debug.Log($"[{character.Data.Name}] が [{characterInHand.Data.Name}] を捨ててアンコールされました。");
                        }
                        else {
                            Debug.Log($"特殊アンコールのコストが払えませんでした（手札にキャラがいない）。");
                        }
                    }

                    if (encored)
                    {
                        state.EventBus.Raise(new GameEvent(new GameEventType("CharacterEncored"), new { Character = character, Type = choice.ToString() }));
                    }
                }
            }

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "EndOfEncoreStep"));
            Debug.Log("アンコールステップ終了");
        }
    }

    /// <summary>
    /// エンドフェイズ。手札の上限調整や、クライマックスカードの片付けを行います。
    /// </summary>
    public class EndPhase : PhaseBase {
        public EndPhase() : base("turn.end", "End Phase") {}
        /// <summary>
        /// フェーズ開始時、手札が7枚を超える場合は超過分を捨てさせ、クライマックス置場のカードを控え室に置きます。
        /// </summary>
        public override void OnEnter(GameState state) {
            base.OnEnter(state);
            var player = state.ActivePlayer as WeissPlayer;

            state.EventBus.Raise(new GameEvent(new GameEventType("CheckTiming"), "StartOfEndPhase"));
            
            // 手札上限チェック（7枚）
            while (player.GetZone<IHandZone<WeissCard>>().Cards.Count > player.HandLimit) {
                // 7枚より多い場合、プレイヤーに捨てさせるカードを選択させる
                var discard = player.Controller.ChooseCardFromHand(player, optional: false);
                player.GetZone<IHandZone<WeissCard>>().RemoveCard(discard);
                player.GetZone<IDiscardPile<WeissCard>>().AddCard(discard);
                state.EventBus.Raise(new GameEvent(new GameEventType("DiscardForHandLimit"), discard));
            }

            // クライマックス置場のカードを控え室に移動
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