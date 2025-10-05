using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツのゲーム進行全体を管理するクラス。
    /// GameStateとRuleEngineを保持し、ゲームのセットアップと開始を担当します。
    /// </summary>
    public class WeissGame : GameBase
    {
        public new WeissRuleEngine RuleEngine { get; protected set; }

        public WeissGame()
        {
            // GameStateはGameBaseのコンストラクタで初期化される
            RuleEngine = new WeissRuleEngine(this.GameState);
        }

        protected override void SetupGame(GameState state)
        {
            // TODO: ゲーム開始時のセットアップロジックを実装する
            // 1. プレイヤーインスタンスを2つ作成する (例: new WeissPlayer("Player1", new ConsolePlayerController()))
            // 2. 各プレイヤーのゾーン（Deck, Hand, Stageなど）をすべて初期化し、Player.RegisterZoneで登録する
            // 3. 各プレイヤーのデッキにカードデータを読み込む
            // 4. 各プレイヤーの山札をシャッフルする (deck.Shuffle())
            // 5. 各プレイヤーが初期手札を5枚引く (deck.DrawTop() x5)
            // 6. 初期手札の引き直し（マリガン）処理を実装する
            // 7. ゲーム開始をイベントバスで通知する
        }
    }
}