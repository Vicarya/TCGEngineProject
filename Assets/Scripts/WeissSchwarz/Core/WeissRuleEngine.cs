using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss 
{
    /// <summary>
    /// ヴァイスシュヴァルツのゲームルールを処理するエンジンクラス。
    /// ゲームの「頭脳」として、GameStateを監視・変更し、ダメージ計算、リフレッシュ処理、
    /// レベルアップ、能力の解決など、複雑なルール判定と実行を担当します。
    /// 全てのゲームイベントを購読し、ルールに基づいた自動処理（例：自動能力の誘発）を行います。
    /// </summary>
    public class WeissRuleEngine {
        public WeissGameState GameState { get; }
        public PhaseBase TurnPhaseTree { get; }

        private WeissGameState _weissState => GameState;

        /// <summary>
        /// 新しいルールエンジンインスタンスを初期化します。
        /// </summary>
        /// <param name="state">監視対象のGameState。</param>
        public WeissRuleEngine(GameState state) {
            GameState = state as WeissGameState;
            if (GameState == null)
            {
                Debug.LogError("WeissRuleEngineはWeissGameStateを必要とします！");
                return;
            }
            // ターンのフェーズ構成を生成
            TurnPhaseTree = WeissPhaseFactory.CreateTurnPhaseTree();
            // ゲーム内で発生する全てのイベントを購読し、HandleGameEventメソッドで処理する
            GameState.EventBus.SubscribeToAll(HandleGameEvent);
        }

        /// <summary>
        /// 全てのゲームイベントを受け取り、関連するルール処理を呼び出すメインハンドラ。
        /// </summary>
        private void HandleGameEvent(GameEvent evt)
        {
            // 1. イベント発生をトリガーとする自動能力があるかチェックする
            CheckForTriggeredAbilities(evt);
            // 2. チェックの結果、キューに追加された能力を解決する
            ResolveAbilityQueue();
        }

        /// <summary>
        /// 指定されたイベントに基づき、誘発する自動能力（【自】）をチェックし、AbilityQueueに追加します。
        /// </summary>
        /// <param name="evt">発生したゲームイベント。</param>
        public void CheckForTriggeredAbilities(GameEvent evt)
        {
            if (_weissState == null) return;

            // TODO: 現状は特定のイベントと能力テキストにのみ対応。将来的には拡張が必要。
            if (evt.Type == BaseGameEvents.CardPlayed)
            {
                // 「カードがプレイされた時」の能力をチェック
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
                                    Debug.Log($"誘発: {playedCard.Data.Name} の登場時能力: {abilityText}");
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
                // 「アタックした時」の能力をチェック
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
                                    Debug.Log($"誘発: {attacker.Data.Name}のアタック時能力: {abilityText}");
                                    var pendingAbility = new PendingAbility(attacker, abilityText, player);
                                    _weissState.AbilityQueue.Add(pendingAbility);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// AbilityQueueに保留中の能力があれば、プレイヤーの選択に従い、順次解決します。
        /// </summary>
        public void ResolveAbilityQueue()
        {
            if (_weissState == null) return;

            // キューに解決すべき能力がなくなるまでループ
            while (true)
            {
                var activePlayer = _weissState.ActivePlayer as WeissPlayer;
                var pendingAbilities = _weissState.AbilityQueue.GetPendingAbilitiesForPlayer(activePlayer);

                if (!pendingAbilities.Any())
                {
                    break; // 解決すべき能力がなければループを抜ける
                }

                PendingAbility abilityToResolve;
                if (pendingAbilities.Count > 1)
                {
                    // 解決すべき能力が複数ある場合、プレイヤーに解決順を選択させる
                    abilityToResolve = activePlayer.Controller.ChooseAbilityToResolve(activePlayer, pendingAbilities);
                }
                else
                {
                    abilityToResolve = pendingAbilities.First();
                }

                // 選択された能力の効果を解決
                ResolveAbilityEffect(abilityToResolve);

                // 解決済みの能力をキューから削除
                _weissState.AbilityQueue.Remove(abilityToResolve);
            }
        }

        /// <summary>
        /// 保留中の能力の効果を具体的に解決します。
        /// </summary>
        /// <param name="ability">解決する能力。</param>
        private void ResolveAbilityEffect(PendingAbility ability)
        {
            Debug.Log($"効果解決中: {ability.AbilityText}");

            // TODO: 現状は特定の能力テキストにのみ対応するハードコード実装。
            // 将来的に、能力テキストをパースして汎用的な効果処理を呼び出すシステム（例: EffectFactory）に置き換えるべき。
            const string onPlayStockAbility = "【自】 このカードが手札から舞台に置かれた時、あなたは自分の山札の上から1枚を、ストック置場に置いてよい。";

            if (ability.AbilityText == onPlayStockAbility)
            {
                var player = ability.ResolvingPlayer;
                
                // プレイヤーに能力を使うかどうか確認
                bool useAbility = player.Controller.AskYesNo(player, $"能力を使いますか？ \"{ability.AbilityText}\" ");
                if (useAbility)
                {
                    var stock = player.GetZone<IStockZone<WeissCard>>();
                    var cardToStock = this.DrawCard(player); // DrawCardはリフレッシュ処理も含む
                    if (cardToStock != null)
                    {
                        stock.AddCard(cardToStock);
                        GameState.EventBus.Raise(new GameEvent(new GameEventType("CardToStock"), new { Card = cardToStock, Source = ability.SourceCard }));
                        Debug.Log($"能力効果: [{cardToStock.Data.Name}] が山札からストックに置かれました。");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"未実装の能力です: {ability.AbilityText}");
            }
        }

        /// <summary>
        /// 指定されたプレイヤーの1ターンを実行します。
        /// </summary>
        /// <param name="turnPlayer">ターンプレイヤー。</param>
        public void ExecuteTurn(Player turnPlayer) {
            GameState.CurrentPlayerIndex = GameState.Players.IndexOf(turnPlayer);
            TurnPhaseTree.Execute(GameState); // スタンドフェイズからエンドフェイズまで順次実行
        }

        /// <summary>
        /// トリガーステップの処理。山札の一番上をめくり、アイコンに応じた効果を処理します。
        /// </summary>
        /// <param name="attacker">アタックしているプレイヤー。</param>
        /// <returns>トリガーによって増加したソウルの値。</returns>
        public int TriggerCheck(Player attacker) {
            var stock = attacker.GetZone<IStockZone<WeissCard>>();
            var weissAttacker = attacker as WeissPlayer;
            if (weissAttacker == null) return 0;

            // 1. 山札を1枚めくる
            var triggeredCard = this.DrawCard(attacker);
            if (triggeredCard == null) return 0; // 山札が引ききれた場合は0

            var cardData = (triggeredCard as WeissCard)?.Data as WeissCardData;
            
            // イベント発行: トリガーカードが公開された
            GameState.EventBus.Raise(new GameEvent(new GameEventType("TriggerReveal"), new { Player = attacker, Card = triggeredCard }));

            int soulBoost = 0;
            if (cardData != null)
            {
                // 2. トリガーアイコンを解析
                var icons = cardData.TriggerIcon?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

                foreach (var icon in icons)
                {
                    // 3. アイコンの種類に応じて効果を処理
                    switch (icon)
                    {
                        case "Soul": // ソウルアイコン
                            soulBoost++;
                            break;
                        case "Comeback": // カムバックアイコン（扉）
                            var waitingRoom = weissAttacker.GetZone<WaitingRoomZone>();
                            var hand = weissAttacker.GetZone<IHandZone<WeissCard>>();
                            var charactersInWaitingRoom = waitingRoom.Cards
                                .Where(c => (((c as WeissCard)?.Data) as WeissCardData)?.CardType == "キャラクター")
                                .ToList();

                            // プレイヤーに回収するキャラを選択させる
                            var characterToReturn = weissAttacker.Controller.ChooseCardFromWaitingRoom(weissAttacker, charactersInWaitingRoom, true);
                            if (characterToReturn != null) {
                                waitingRoom.RemoveCard(characterToReturn);
                                hand.AddCard(characterToReturn);
                            }
                            break;
                        case "Draw": // ドローアイコン（風）
                            var drawnCard = this.DrawCard(attacker);
                            if (drawnCard != null) {
                                attacker.GetZone<IHandZone<WeissCard>>().AddCard(drawnCard);
                            }
                            break;
                        // TODO: 他のアイコン（袋、本、宝、門）の処理を実装
                    }
                }
            }
            
            // 4. めくったカードはストック置場に置かれる
            stock.AddCard(triggeredCard);
            
            if(soulBoost > 0) Debug.Log($"トリガー！ ソウル+{soulBoost}");

            return soulBoost;
        }

        /// <summary>
        /// ダメージ処理。指定された量のダメージをプレイヤーに与えます。
        /// </summary>
        /// <param name="victim">ダメージを受けるプレイヤー。</param>
        /// <param name="amount">ダメージの量。</param>
        public void ApplyDamage(Player victim, int amount) {
            var clock = victim.GetZone<IClockZone<WeissCard>>();
            var waitingRoom = victim.GetZone<WaitingRoomZone>();

            for (int i=0;i<amount;i++) {
                // 1. 山札から1枚めくる
                var card = this.DrawCard(victim);
                if (card == null) return; // 山札がない場合は処理中断

                var weissCardData = (card as WeissCard)?.Data as WeissCardData;
                // 2. めくったカードがクライマックスならダメージキャンセル
                if (weissCardData != null && weissCardData.CardType == WeissCardType.Climax.ToString()) {
                    Debug.Log($"ダメージキャンセル！ トリガーしたカード: {weissCardData.Name}");
                    waitingRoom.AddCard(card); // キャンセルしたカードは控え室へ
                    GameState.EventBus.Raise(new GameEvent(WeissGameEvents.DamageCancelled, new { Player = victim, Card = card }));
                    break; // ダメージ処理を中断
                }

                // 3. クライマックスでなければ、クロックに置く
                clock.AddCard(card);
                GameState.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = victim, Card = card }));
                
                // 4. レベルアップの条件を満たしているかチェック
                CheckAndHandleLevelUp(victim);
            }
        }

        /// <summary>
        /// プレイヤーのクロックが7枚以上かチェックし、条件を満たしていればレベルアップ処理を行います。
        /// </summary>
        private void CheckAndHandleLevelUp(Player player) {
            var clock = player.GetZone<ClockZone>();
            var level = player.GetZone<LevelZone>();
            var waitingRoom = player.GetZone<WaitingRoomZone>();

            // クロックが7枚以上ある限りレベルアップ処理を繰り返す
            while (clock.Cards.Count >= 7)
            {
                Debug.Log($"{player.Name}がレベルアップ！");
                var cardsForLevelUp = clock.Cards.Take(7).ToList();

                // 1. プレイヤーにレベルアップに使うカードを1枚選ばせる
                var chosen = (player as WeissPlayer).Controller.ChooseLevelUpCard(player as WeissPlayer, cardsForLevelUp);

                // 2. 選んだカードをレベル置場へ
                level.AddCard(chosen);
                GameState.EventBus.Raise(new GameEvent(new GameEventType("LevelUp"), new { Player = player, Card = chosen }));

                // 3. 残りの6枚を控え室へ
                clock.RemoveCards(cardsForLevelUp);
                foreach (var c in cardsForLevelUp.Where(card => card != chosen)) {
                    waitingRoom.AddCard(c);
                }
            }
        }

        /// <summary>
        /// リフレッシュペナルティによる1ダメージを処理します。
        /// </summary>
        private void ApplyRefreshPointDamage(Player player)
        {
            var clock = player.GetZone<IClockZone<WeissCard>>();
            Debug.Log($"{player.Name}がリフレッシュペナルティを受ける。");

            var damageCard = this.DrawCard(player);
            if (damageCard == null) return; // ドローできなかった場合は処理終了

            // リフレッシュペナルティのダメージはキャンセルされない
            clock.AddCard(damageCard);
            GameState.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = player, Card = damageCard, IsRefreshPoint = true }));

            CheckAndHandleLevelUp(player);
        }

        /// <summary>
        /// プレイヤーの山札からカードを1枚引きます。山札が0枚の場合はリフレッシュ処理を実行します。
        /// </summary>
        /// <returns>引いたカード。引けなかった場合はnull。</returns>
        public WeissCard DrawCard(Player player)
        {
            var deck = player.GetZone<IDeckZone<WeissCard>>();
            var card = deck.DrawTop();

            if (card == null)
            {
                // 山札が0枚なのでリフレッシュ処理
                Debug.Log($"{player.Name}の山札が0枚です。リフレッシュ処理を実行します。");

                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
                if (waitingRoom.Cards.Count == 0)
                {
                    // 控え室にもカードがない場合、続行不能
                    Debug.LogWarning($"{player.Name}は山札も控え室も0枚です。");
                    return null;
                }
                
                // 1. 控え室のカードを全て山札に戻す
                var cardsToReturn = new List<WeissCard>(waitingRoom.Cards);
                foreach(var c in cardsToReturn)
                {
                    waitingRoom.RemoveCard(c);
                    (deck as DeckZone)?.AddCard(c);
                }

                // 2. 山札をシャッフル
                deck.Shuffle();
                GameState.EventBus.Raise(new GameEvent(WeissGameEvents.DeckRefresh, player));
                Debug.Log($"{player.Name}が{cardsToReturn.Count}枚のカードで山札をリフレッシュしました。");

                // 3. リフレッシュペナルティとして1ダメージ
                ApplyRefreshPointDamage(player);

                // 4. 改めて山札から1枚引く
                card = deck.DrawTop();
            }

            return card;
        }

        /// <summary>
        /// 起動能力（【起】）の効果を解決します。
        /// </summary>
        public void ActivateAbility(WeissCard card, string abilityText)
        {
            Debug.Log($"能力起動: [{card.Data.CardCode}] {abilityText}");

            // TODO: ハードコード実装。将来的には拡張が必要。
            const string targetAbilityText = "【起】 あなたは自分のキャラを1枚選び、【レスト】する。そうしたら、あなたは1枚引く。";

            if (abilityText == targetAbilityText)
            {
                var player = card.Owner as WeissPlayer;
                if (player == null) return;
                
                // コストとして自分のキャラを1枚レストさせる
                var stage = player.GetZone<IStageZone<WeissCard>>();
                var validTargets = stage.Cards.Where(c => c != null && !c.IsRested).ToList();
                var targetToRest = player.Controller.ChooseTargetCard(player, validTargets, "コストとしてレストする自分のキャラを1枚選んでください。", false);

                if (targetToRest != null)
                {
                    // コスト支払い
                    targetToRest.Rest();
                    Debug.Log($"コストとして[{targetToRest.Data.Name}]をレストしました。");

                    // 効果解決: 1枚引く
                    var hand = player.GetZone<IHandZone<WeissCard>>();
                    var drawnCard = this.DrawCard(player);
                    if (drawnCard != null)
                    {
                        hand.AddCard(drawnCard);
                        Debug.Log($"効果でカードを1枚引きました。");
                    }
                    
                    GameState.EventBus.Raise(new GameEvent(new GameEventType("AbilityResolved"), new { Player = card.Owner, Card = card, Effect = abilityText }));
                }
                else
                {
                    Debug.Log("コストを支払えませんでした。能力の解決に失敗しました。");
                }
            }
            else
            {
                Debug.LogWarning($"未実装の起動能力です: {abilityText}");
            }
        }

        /// <summary>
        /// 助太刀（カウンター）能力の効果を解決します。
        /// </summary>
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
            if (assistText == null) return;

            // 1. 正規表現で能力テキストからパワーとレベル要件を抽出
            var match = System.Text.RegularExpressions.Regex.Match(assistText, @"【助太刀(?<power>\d+)\s+レベル(?<level>\d+)】");
            if (match.Success && int.TryParse(match.Groups["power"].Value, out int powerBoost) && int.TryParse(match.Groups["level"].Value, out int levelReq))
            {
                var player = defendingCharacter.Owner as WeissPlayer;
                // 2. レベル要件チェック
                if (player.GetZone<LevelZone>().Count < levelReq) return;

                // 3. コスト支払い
                if (!PayCounterCost(counterCard)) return;

                // 4. 効果解決: 対象キャラのパワーを上げる
                defendingCharacter.TemporaryPower += powerBoost;
                Debug.Log($"助太刀効果: [{defendingCharacter.Data.Name}]のパワーが+{powerBoost}されました。");
            }
            else
            {
                Debug.LogError($"助太刀能力の解析に失敗しました: {assistText}");
            }
        }

        /// <summary>
        /// 助太刀のコスト（1ストックと自身を手札から控え室へ）を支払います。
        /// </summary>
        private bool PayCounterCost(WeissCard counterCard)
        {
            var player = counterCard.Owner as WeissPlayer;
            var stock = player.GetZone<IStockZone<WeissCard>>();
            var hand = player.GetZone<IHandZone<WeissCard>>();
            var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();

            // ストックコスト
            if (stock.Cards.Count < 1) return false;
            var stockCard = stock.RemoveTopCard();
            if (stockCard != null) waitingRoom.AddCard(stockCard);

            // 手札コスト（助太刀カード自身）
            hand.RemoveCard(counterCard);
            waitingRoom.AddCard(counterCard);

            Debug.Log($"助太刀コスト支払い: 1ストックと手札の[{counterCard.Data.Name}]");
            return true;
        }
    }
}