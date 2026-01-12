using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツのゲーム進行全体を管理する、ゲームロジックの最上位クラス。
    /// GameCoreの汎用的なGameBaseを継承し、ヴァイスシュヴァルツ固有のルールを実装します。
    /// ゲームの準備（プレイヤー、ゾーン、デッキの作成）と、マリガンなどの主要なゲーム進行を担当します。
    /// </summary>
    public class WeissGame : GameBase
    {
        /// <summary>
        /// ヴァイスシュヴァルツ専用のGameStateへの便利なアクセスを提供します。
        /// </summary>
        public new WeissGameState GameState => base.GameState as WeissGameState;
        
        /// <summary>
        /// ヴァイスシュヴァルツの複雑なルール判定を担当するエンジン。
        /// </summary>
        public WeissRuleEngine RuleEngine { get; protected set; }

        /// <summary>
        /// WeissGameの新しいインスタンスを初期化します。
        /// </summary>
        public WeissGame()
        {
            // GameStateは基底クラスのコンストラクタからCreateGameState()経由で初期化される
            RuleEngine = new WeissRuleEngine(this.GameState);
        }

        /// <summary>
        /// 基底クラスの初期化プロセス中に呼び出され、このゲーム専用のGameStateインスタンスを生成して返します。
        /// </summary>
        /// <returns>新しいWeissGameStateインスタンス。</returns>
        protected override GameState CreateGameState()
        {
            return new WeissGameState(this);
        }

        /// <summary>
        /// ゲーム開始前のマリガンフェイズ（手札交換）を非同期で実行します。
        /// </summary>
        public async Task PerformMulliganPhase()
        {
            foreach (WeissPlayer player in GameState.Players)
            {
                var handZone = player.GetZone<IHandZone<WeissCard>>();
                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
                var deckZone = player.GetZone<IDeckZone<WeissCard>>();

                // プレイヤーコントローラーに手札交換の選択を非同期で要求し、待機する
                var cardsToMulligan = await player.Controller.ChooseMulliganCards(player, new List<WeissCard>(handZone.Cards));

                if (cardsToMulligan != null && cardsToMulligan.Count > 0)
                {
                    // 1. 選択されたカードを手札から控え室に移動
                    foreach (var card in cardsToMulligan)
                    {
                        handZone.RemoveCard(card);
                        waitingRoom.AddCard(card);
                    }

                    // 2. 移動した枚数と同じだけ、山札からカードを引く
                    for (int i = 0; i < cardsToMulligan.Count; i++)
                    {
                        var newCard = deckZone.DrawTop();
                        if (newCard != null) handZone.AddCard(newCard);
                    }
                }
            }
        }

        /// <summary>
        /// ゲームの初期セットアップを実行します。プレイヤー、ゾーン、デッキなどを準備します。
        /// </summary>
        /// <param name="state">セットアップ対象のGameState。</param>
        protected override void SetupGame(GameState state)
        {
            // 手順1: カードデータベースからすべてのカードデータをロードする
            CardLoader.LoadAllCardAssets();

            // 手順2: プレイヤーインスタンスを2つ作成する (デバッグ用にConsoleControllerを使用)
            var player1 = new WeissPlayer("Player1", new ConsolePlayerController());
            var player2 = new WeissPlayer("Player2", new ConsolePlayerController());

            state.Players.Add(player1);
            state.Players.Add(player2);

            foreach (WeissPlayer player in new[] { player1, player2 })
            {
                // 手順3: 各プレイヤーのゾーン（レベル置場、控え室など）をすべて初期化し、プレイヤーに登録する
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

                player.RegisterZone<ILevelZone<WeissCard>>(levelZone);
                player.RegisterZone<IDiscardPile<WeissCard>>(waitingRoom);
                player.RegisterZone<IClockZone<WeissCard>>(clockZone);
                player.RegisterZone<IDeckZone<WeissCard>>(deckZone);
                player.RegisterZone<IHandZone<WeissCard>>(handZone);
                player.RegisterZone<IStageZone<WeissCard>>(stageZone);
                player.RegisterZone<IStockZone<WeissCard>>(stockZone);
                player.RegisterZone<IClimaxZone<WeissCard>>(climaxZone);
                player.RegisterZone<MemoryZone>(memoryZone);
                player.RegisterZone<ResolutionZone>(resolutionZone);

                // 手順4: 各プレイヤーのデッキをファイルから読み込む
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
                    Debug.LogWarning($"デッキファイルが見つかりません: {deckFilePath}。サンプルデッキを使用します。");
                    // ファイルが見つからない場合、フォールバックとしてサンプルデッキを生成
                    foreach (var cardData in CardLoader.AllCards.Take(10))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            deckZone.AddCard(new WeissCard(cardData, player));
                        }
                    }
                }

                // 手順5: 各プレイヤーの山札をシャッフルする
                deckZone.Shuffle();

                // 手順6: 各プレイヤーが初期手札を5枚引く
                for (int i = 0; i < 5; i++)
                {
                    var card = deckZone.DrawTop();
                    if (card != null) handZone.AddCard(card);
                }
            }
            
            // 手順7: ゲーム開始をイベントバスで通知する (最初のプレイヤーのターン開始)
            state.EventBus.Raise(new GameEvent(BaseGameEvents.TurnStarted, state.Players[0]));
        }
    }
}