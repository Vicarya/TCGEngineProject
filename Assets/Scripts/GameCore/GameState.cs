using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// TCGのゲーム全体の現在の状態を保持するクラス。
    /// ターン、フェーズ、プレイヤー、イベントバスなど、ゲーム進行に必要な全ての情報を含みます。
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// 現在のゲームロジックを管理するGameBaseのインスタンスを取得します。
        /// </summary>
        public GameBase Game { get; }

        /// <summary>
        /// ゲーム内イベントの発行と購読を管理するイベントバスを取得します。
        /// </summary>
        public EventBus EventBus { get; }

        /// <summary>
        /// 現在のフェーズのIDを取得または設定します。
        /// </summary>
        public string CurrentPhaseId { get; set; }

        /// <summary>
        /// 現在のプレイヤーのインデックスを取得または設定します。
        /// </summary>
        public int CurrentPlayerIndex { get; set; } = 0;

        /// <summary>
        /// 現在のターン数を取得または設定します。
        /// </summary>
        public int TurnCounter { get; set; } = 0;

        /// <summary>
        /// 現在アクティブなプレイヤーを取得します。
        /// </summary>
        public Player ActivePlayer => Players.Count > CurrentPlayerIndex ? Players[CurrentPlayerIndex] : null;

        /// <summary>
        /// ゲームに参加している全プレイヤーのリストを取得します。
        /// </summary>
        public List<Player> Players { get; } = new();

        /// <summary>
        /// GameStateクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="game">このステートが属するGameBaseインスタンス。</param>
        public GameState(GameBase game)
        {
            Game = game;
            EventBus = new EventBus();
        }
    }
}
