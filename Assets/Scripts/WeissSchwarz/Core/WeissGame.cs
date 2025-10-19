using TCG.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツのゲーム進行全体を管理するクラス。
    /// GameStateとRuleEngineを保持し、ゲームのセットアップと開始を担当します。
    /// </summary>
    public class WeissGame : GameBase
    {
        public WeissRuleEngine RuleEngine { get; protected set; }

        public WeissGame()
        {
            // GameStateはGameBaseのコンストラクタで初期化される
            RuleEngine = new WeissRuleEngine(this.GameState);
        }

        protected override void SetupGame(GameState state)
        {
            // 1. カードデータベースからカードを読み込む
            CardLoader.LoadCards(Application.streamingAssetsPath + "/WeissSchwarz/cards.json");

            // 2. プレイヤーインスタンスを2つ作成する
            var player1 = new WeissPlayer("Player1", new ConsolePlayerController());
            var player2 = new WeissPlayer("Player2", new ConsolePlayerController());

            state.Players.Add(player1);
            state.Players.Add(player2);

            foreach (WeissPlayer player in new[] { player1, player2 })
            {
                // 3. 各プレイヤーのゾーンをすべて初期化し、登録する
                var levelZone = new LevelZone(player);
                var waitingRoom = new WaitingRoomZone(player);
                var clockZone = new ClockZone(player, levelZone, waitingRoom);
                var deckZone = new DeckZone(player);
                var handZone = new HandZone(player);
                var stageZone = new StageZone(player);
                var stockZone = new StockZone(player);
                var climaxZone = new ClimaxZone(player);
                var memoryZone = new MemoryZone(player);
                var resolutionZone = new ResolutionZone(player);

                player.RegisterZone<LevelZone>(levelZone);
                player.RegisterZone<IDiscardPile<WeissCard>>(waitingRoom);
                player.RegisterZone<IClockZone<WeissCard>>(clockZone);
                player.RegisterZone<IDeckZone<WeissCard>>(deckZone);
                player.RegisterZone<IHandZone<WeissCard>>(handZone);
                player.RegisterZone<IStageZone<WeissCard>>(stageZone);
                player.RegisterZone<IStockZone<WeissCard>>(stockZone);
                player.RegisterZone<IClimaxZone<WeissCard>>(climaxZone);
                player.RegisterZone<MemoryZone>(memoryZone);
                player.RegisterZone<ResolutionZone>(resolutionZone);

                // 4. 各プレイヤーのデッキをファイルから読み込む
                string deckFilePath = Application.streamingAssetsPath + "/WeissSchwarz/Decks/deck1.txt";
                if (System.IO.File.Exists(deckFilePath))
                {
                    var deckList = System.IO.File.ReadAllLines(deckFilePath);
                    foreach (string line in deckList)
                    {
                        var parts = line.Split(' ');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int quantity))
                        {
                            string cardCode = parts[0];
                            var cardData = CardLoader.AllCards.FirstOrDefault(c => c.CardCode == cardCode);
                            if (cardData != null)
                            {
                                for (int i = 0; i < quantity; i++)
                                {
                                    deckZone.AddCard(new WeissCard(cardData, player));
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Deck file not found at {deckFilePath}. Using sample deck.");
                    // Fallback to sample deck if file not found
                    foreach (var cardData in CardLoader.AllCards.Take(10))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            deckZone.AddCard(new WeissCard(cardData, player));
                        }
                    }
                }

                // 5. 各プレイヤーの山札をシャッフルする
                deckZone.Shuffle();

                // 6. 各プレイヤーが初期手札を5枚引く
                for (int i = 0; i < 5; i++)
                {
                    var card = deckZone.DrawTop();
                    if (card != null) handZone.AddCard(card);
                }
            }

            // 6.5. 初期手札の引き直し（マリガン）処理
            foreach (WeissPlayer player in state.Players)
            {
                var handZone = player.GetZone<IHandZone<WeissCard>>();
                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
                var deckZone = player.GetZone<IDeckZone<WeissCard>>();

                var cardsToMulligan = player.Controller.ChooseMulliganCards(player, new List<WeissCard>(handZone.Cards));
                
                if (cardsToMulligan != null && cardsToMulligan.Count > 0)
                {
                    // 選択されたカードを控え室に送り、同数をドローする
                    foreach (var card in cardsToMulligan)
                    {
                        handZone.RemoveCard(card);
                        waitingRoom.AddCard(card);
                    }

                    for (int i = 0; i < cardsToMulligan.Count; i++)
                    {
                        var newCard = deckZone.DrawTop();
                        if (newCard != null) handZone.AddCard(newCard);
                    }
                }
            }

            // 7. ゲーム開始をイベントバスで通知する (最初のプレイヤーのターン開始)
            state.EventBus.Raise(new GameEvent(BaseGameEvents.TurnStarted, state.Players[0]));
        }
    }
}