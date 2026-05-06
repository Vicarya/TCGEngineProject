using UnityEngine;
using TCG.Core;
using TCG.Weiss.UI;

namespace TCG.Weiss
{
    /// <summary>
    /// Unity シーン上で WeissGame のライフサイクルを管理するエントリポイント。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// GameManager のシングルトンインスタンス。
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// 実行中のゲームインスタンス。
        /// </summary>
        public WeissGame Game => _game;

        private WeissGame _game;
        private bool _isTurnExecuting;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _game = new WeissGame();
        }

        private async void Start()
        {
            try
            {
                // 起動順による null を防ぐため、先に DB を初期化しておく。
                WeissCardDatabase.EnsureInitialized();

                _game.StartGame();
                RefreshView();
                _game.GameState.EventBus.SubscribeToAll(_ => RefreshView());
                await _game.PerformMulliganPhase();

                // 初期表示を同期
                RefreshView();

                // 1ターン目を実行
                await ExecuteCurrentTurnAsync();

                Debug.Log("Game Manager が起動し、ゲームを初期化しました。");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Game Manager: 初期化中に致命的なエラーが発生したため、進行を停止しました。");
            }
        }

        private void Update()
        {
            _game?.Update();
        }

        /// <summary>
        /// 現在のターンプレイヤーの1ターンを実行し、終了後に画面を更新する。
        /// </summary>
        private async System.Threading.Tasks.Task ExecuteCurrentTurnAsync()
        {
            if (_isTurnExecuting || _game == null || _game.GameState == null || _game.RuleEngine == null)
            {
                return;
            }

            var active = _game.GameState.ActivePlayer;
            if (active == null)
            {
                Debug.LogWarning("GameManager: ActivePlayer is null. Turn execution skipped.");
                return;
            }

            _isTurnExecuting = true;
            try
            {
                await _game.RuleEngine.ExecuteTurnAsync(active);
            }
            finally
            {
                _isTurnExecuting = false;
                RefreshView();
            }
        }

        private void RefreshView()
        {
            if (_game?.GameState == null || GameView.Instance == null)
            {
                return;
            }

            var active = _game.GameState.ActivePlayer;
            if (active != null)
            {
                GameView.Instance.UpdateView(active);
            }
        }
    }
}
