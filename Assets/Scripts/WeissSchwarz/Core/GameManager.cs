
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// Unityのシーンに配置され、ゲーム全体のライフサイクルを管理するMonoBehaviour。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public WeissGame Game { get { return _game; } }
        private WeissGame _game;

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

        void Start()
        {
            // ゲームインスタンスを作成し、セットアップとゲーム開始をトリガーする
            _game = new WeissGame();
            _game.StartGame();

            Debug.Log("Game Manager Started and Game Initialized.");
        }

        void Update()
        {
            // ゲームのメインループを毎フレーム実行する
            _game?.Update();
        }
    }
}
