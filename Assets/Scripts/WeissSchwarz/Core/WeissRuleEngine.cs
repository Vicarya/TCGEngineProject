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
    /// ヴァイスシュヴァルツのゲームルールを管理し、実行するエンジンクラス。
    /// ダメージ処理、レベルアップ、トリガーチェック、リフレッシュなど、ゲームの根幹となるロジックを担当します。
    /// </summary>
    public class WeissRuleEngine {
        public GameState State { get; }
        public PhaseBase TurnPhaseTree { get; }
        public WeissRuleEngine(GameState state) {
            State = state;
            TurnPhaseTree = WeissPhaseFactory.CreateTurnPhaseTree();
        }

        /// <summary>
        /// 指定されたプレイヤーのターンを同期的に実行します。
        /// </summary>
        public void ExecuteTurn(Player turnPlayer) {
            State.CurrentPlayerIndex = State.Players.IndexOf(turnPlayer);
            TurnPhaseTree.Execute(State);
        }

        /// <summary>
        /// アタック宣言時のトリガーチェックを実行します。
        /// 山札の一番上のカードを公開し、トリガーアイコンに応じた処理（ここではストックに送る）を行います。
        /// </summary>
        /// <param name="attacker">攻撃プレイヤー</param>
        public int TriggerCheck(Player attacker) {
            var deck = attacker.GetZone<IDeckZone<WeissCard>>();
            var stock = attacker.GetZone<IStockZone<WeissCard>>();
            var weissAttacker = attacker as WeissPlayer;
            if (weissAttacker == null) return 0;

            var triggeredCard = this.DrawCard(attacker);
            if (triggeredCard == null) {
                // DrawCard handles the empty deck case, but if it still returns null, something is wrong (e.g., lose condition)
                return 0;
            }

            var cardData = (triggeredCard as WeissCard)?.Data as WeissCardData;
            if (cardData == null) {
                stock.AddCard(triggeredCard);
                return 0;
            }

            State.EventBus.Raise(new GameEvent(new GameEventType("TriggerReveal"), new { Player = attacker, Card = triggeredCard }));

            int soulBoost = 0;
            var icons = cardData.TriggerIcon?.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            foreach (var icon in icons) {
                switch (icon) {
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
                            State.EventBus.Raise(new GameEvent(new GameEventType("TriggerComeback"), new { Player = attacker, ReturnedCard = characterToReturn }));
                        }
                        break;
                    case "Draw":
                        var handZone = attacker.GetZone<IHandZone<WeissCard>>();
                        var drawnCard = deck.DrawTop();
                        if (drawnCard != null) {
                            handZone.AddCard(drawnCard);
                            State.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = attacker, Card = drawnCard }));
                        }
                        break;
                }
            }

            stock.AddCard(triggeredCard);
            
            if(soulBoost > 0) Debug.Log($"Triggered {soulBoost} SOUL!");

            return soulBoost;
        }

        /// <summary>
        /// プレイヤーに指定された点数のダメージを与えます。
        /// 山札の上からカードをクロックゾーンに置き、必要であればリフレッシュ処理やレベルアップ処理を呼び出します。
        /// </summary>
        /// <param name="victim">ダメージを受けるプレイヤー</param>
        /// <param name="amount">ダメージ量</param>
        public void ApplyDamage(Player victim, int amount) {
            var deck = victim.GetZone<IDeckZone<WeissCard>>();
            var clock = victim.GetZone<IClockZone<WeissCard>>();
            var waitingRoom = victim.GetZone<WaitingRoomZone>();
            if (waitingRoom == null) {
                Debug.LogError("WaitingRoomZoneが見つかりません。");
                return;
            }

            for (int i=0;i<amount;i++) {
                var card = this.DrawCard(victim);
                if (card == null) {
                    // Lose condition is handled by DrawCard, just stop the damage process.
                    return;
                }

                // ダメージキャンセルチェック
                var weissCardData = (card as WeissCard)?.Data as WeissCardData;
                if (weissCardData != null && weissCardData.CardType == WeissCardType.Climax.ToString()) {
                    Debug.Log($"ダメージがキャンセルされました！ トリガーしたカード: {weissCardData.Name}");
                    waitingRoom.AddCard(card);
                    State.EventBus.Raise(new GameEvent(new GameEventType("DamageCancelled"), new { Player = victim, Card = card }));
                    break; // ダメージ処理を中断
                }

                clock.AddCard(card);
                State.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = victim, Card = card }));
                
                // ダメージ処理後にレベルアップチェック
                CheckAndHandleLevelUp(victim);
            }
        }

        /// <summary>
        /// クロックが7枚以上の場合、レベルアップ処理を実行します。
        /// </summary>
        /// <param name="player">チェック対象のプレイヤー</param>
        private void CheckAndHandleLevelUp(Player player) {
            var clock = player.GetZone<ClockZone>();
            var level = player.GetZone<LevelZone>();
            var waitingRoom = player.GetZone<WaitingRoomZone>();

            // クロックが7枚以上ある限り処理を繰り返す
            while (clock.Cards.Count >= 7)
            {
                // ヴァイスシュヴァルツのルールでは、クロックは置かれた順（古いものが下）
                // よってリストの先頭から7枚が対象
                var cardsForLevelUp = clock.Cards.Take(7).ToList();

                // TODO: [PlayerController] プレイヤーにレベルアップに使うカードを1枚選ばせる (IPlayerController経由で)
                // 仮実装: 最初の1枚を選択
                var chosen = cardsForLevelUp[0];

                level.AddCard(chosen);
                State.EventBus.Raise(new GameEvent(new GameEventType("LevelUp"), new { Player = player, Card = chosen }));

                // 残りのカードをクロックから削除し、控え室へ送る
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
                State.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
                return;
            }

            // Refresh point damage cannot be cancelled
            clock.AddCard(damageCard);
            State.EventBus.Raise(new GameEvent(new GameEventType("DamageTaken"), new { Player = player, Card = damageCard, IsRefreshPoint = true }));
            Debug.Log($"[{damageCard.Data.Name}] was placed in clock for refresh point damage.");

            // Check for level up
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
                    State.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
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
                State.EventBus.Raise(new GameEvent(new GameEventType("DeckRefresh"), player));
                Debug.Log($"{player.Name} refreshed their deck with {cardsToReturn.Count} cards.");

                ApplyRefreshPointDamage(player);

                card = deck.DrawTop();
                if (card == null)
                {
                    State.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), player));
                    Debug.LogError($"{player.Name}'s deck is still empty after refresh. This should not happen.");
                    return null;
                }
            }

            return card;
        }

        /// <summary>
        /// 指定されたカードの能力を起動します。
        /// </summary>
        /// <param name="card">能力を起動するカード</param>
        /// <param name="abilityText">起動する能力のテキスト</param>
        public void ActivateAbility(WeissCard card, string abilityText)
        {
            Debug.Log($"Activating ability for [{card.Data.CardCode} {card.Data.Name}]: {abilityText}");

            // 1. Parse ability text
            // A simple parser for "【起】 (Cost)：(Effect)" format
            string costText = string.Empty;
            string effectText = string.Empty;

            var match = System.Text.RegularExpressions.Regex.Match(abilityText, @"^【起】(?<cost>.*)：(?<effect>.*)$");
            if (match.Success)
            {
                costText = match.Groups["cost"].Value.Trim();
                effectText = match.Groups["effect"].Value.Trim();
            }
            else
            {
                Debug.LogError($"Could not parse ability text: {abilityText}");
                return;
            }

            // 2. Pay cost
            bool costPaid = false;
            if (costText == "このカードを【レスト】する")
            {
                if (!card.IsRested)
                {
                    card.Rest();
                    costPaid = true;
                    State.EventBus.Raise(new GameEvent(new GameEventType("AbilityCostPaid"), new { Player = card.Owner, Card = card, Cost = costText }));
                }
                else
                {
                    Debug.Log("Cost not met: Card is already rested.");
                }
            }
            else
            {
                Debug.LogWarning($"Unknown cost: {costText}");
            }

            // 3. Resolve effect
            if (costPaid)
            {
                if (effectText == "あなたは1枚引く")
                {
                    var player = card.Owner as WeissPlayer;
                    var hand = player.GetZone<IHandZone<WeissCard>>();
                    var drawnCard = this.DrawCard(player);
                    if (drawnCard != null)
                    {
                        hand.AddCard(drawnCard);
                        State.EventBus.Raise(new GameEvent(BaseGameEvents.CardDrawn, new { Player = player, Card = drawnCard }));
                        Debug.Log($"{player.Name} drew a card due to ability effect.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Unknown effect: {effectText}");
                }

                State.EventBus.Raise(new GameEvent(new GameEventType("AbilityResolved"), new { Player = card.Owner, Card = card, Effect = effectText }));
            }
        }

        public void ResolveCounterAbility(WeissCard counterCard, WeissCard defendingCharacter)
        {
            if (counterCard == null || defendingCharacter == null) return;

            // 1. Find the Assist ability text
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

            // 2. Parse the ability for power
            // Example: 【助太刀2000 レベル1】
            var match = System.Text.RegularExpressions.Regex.Match(assistText, @"【助太刀(?<power>\d+)");
            if (match.Success && int.TryParse(match.Groups["power"].Value, out int powerBoost))
            {
                // TODO: Check level requirement
                // TODO: Pay ability cost

                // 3. Apply the effect
                defendingCharacter.TemporaryPower += powerBoost;
                State.EventBus.Raise(new GameEvent(new GameEventType("PowerBoosted"), new { 
                    Target = defendingCharacter, 
                    Amount = powerBoost, 
                    Source = counterCard 
                }));
                Debug.Log($"[{defendingCharacter.Data.Name}] power boosted by {powerBoost}. New temporary power: {defendingCharacter.TemporaryPower}");
            }
            else
            {
                Debug.LogError($"Could not parse power from assist ability: {assistText}");
            }
        }
    }
}