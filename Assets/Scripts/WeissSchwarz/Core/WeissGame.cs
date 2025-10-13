using TCG.Core;
using System.Collections.Generic;
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

                // 4. 各プレイヤーのデッキにカードデータを読み込む (サンプルとして全カードを5枚ずつ追加)
                foreach (var cardData in CardLoader.AllCards)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        deckZone.AddCard(new WeissCard(cardData, player));
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

            // 7. ゲーム開始をイベントバスで通知する (最初のプレイヤーのターン開始)
            state.EventBus.Raise(new GameEvent(BaseGameEvents.TurnStarted, state.Players[0]));

            // TODO: 初期手札の引き直し（マリガン）処理を実装する
        }
    }
}