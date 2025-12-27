
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// Unityのシーンに配置され、ゲーム全体のライフサイクルを管理するシングルトンMonoBehaviour。
    /// WeissGameのインスタンスを生成・保持し、Unityのライフサイクル（Start, Update）とゲームロジックのループを結びつける役割を持つ。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// GameManagerのシングルトンインスタンス。
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// 現在進行中のゲームインスタンス（WeissGame）への参照。
        /// </summary>
        public WeissGame Game { get { return _game; } }
        
        // ゲームロジックの本体となるWeissGameのインスタンス。
        private WeissGame _game;

        /// <summary>
        /// オブジェクトの初期化時に呼び出され、シングルトンパターンを実装します。
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        /// <summary>
        /// 最初のフレーム更新前に呼び出され、ゲームの非同期初期化処理を実行します。
        /// async voidを使用し、awaitで非同期のマリガン処理の完了を待機します。
        /// </summary>
        async void Start()
        {
            // ゲームインスタンスを作成し、セットアップを実行
            _game = new WeissGame();

            // 非同期でマリガン処理を実行し、完了を待つ
            await _game.PerformMulliganPhase();

            // ゲームを開始する
            _game.StartGame();

            Debug.Log("Game Managerが起動し、ゲームが初期化されました。");
        }

        /// <summary>
        /// 毎フレーム呼び出され、ゲームのメインループを駆動します。
        /// </summary>
        void Update()
        {
            // WeissGameのUpdateメソッドを呼び出すことで、ゲームの状態を更新し続ける。
            _game?.Update();
        }
    }
}
