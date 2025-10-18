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
        public void TriggerCheck(Player attacker) {
            var deck = attacker.GetZone<IDeckZone<WeissCard>>();
            var top = (deck as DeckZone)?.PeekTop();
            if (top == null) return;
            // reveal
            State.EventBus.Raise(new GameEvent(new GameEventType("TriggerReveal"), new { Player = attacker, Card = top }));
            // In many cases: the revealed card then goes to stock (or resolution actions vary)
            // For simplicity: move to Stock
            var drawn = deck.DrawTop();
            if (drawn != null) attacker.GetZone<IStockZone<WeissCard>>().AddCard(drawn);
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
            for (int i=0;i<amount;i++) {
                var card = deck.DrawTop();
                if (card == null) {
                    // deck empty => refresh from waiting room (shuffling) per rule (simplified)
                    var waitingRoom = victim.GetZone<WaitingRoomZone>();
                    var clockZone = victim.GetZone<ClockZone>();
                    if (waitingRoom != null && waitingRoom.Cards.Any()) {
                        (deck as DeckZone)?.RefreshFrom(waitingRoom, clockZone);
                        State.EventBus.Raise(new GameEvent(WeissGameEvents.DeckRefresh, victim));
                    } 
                    else {
                        // lose condition if no cards anywhere
                        State.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), victim));
                        return;
                    }
                    card = deck.DrawTop(); // リフレッシュ後に再度ドロー
                    if (card == null) { State.EventBus.Raise(new GameEvent(new GameEventType("DeckEmptyLose"), victim)); return; } // それでも引けなければ敗北
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
                    var deck = player.GetZone<IDeckZone<WeissCard>>();
                    var hand = player.GetZone<IHandZone<WeissCard>>();
                    var drawnCard = deck.DrawTop();
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
    }
}