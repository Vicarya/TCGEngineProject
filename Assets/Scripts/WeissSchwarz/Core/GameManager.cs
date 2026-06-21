using TCG.Core;
using TCG.Weiss.UI;
using UnityEngine;

namespace TCG.Weiss
{
    public class GameManager : MonoBehaviour
    {
        [Header("Battle Setup")]
        [SerializeField] private string player1Name = "Player1";
        [SerializeField] private string player2Name = "Player2";
        [SerializeField] private PlayerControlMode player1ControlMode = PlayerControlMode.Human;
        [SerializeField] private PlayerControlMode player2ControlMode = PlayerControlMode.Npc;
        [SerializeField] private string player1DeckId = WeissBattleSetupOptions.DefaultSceneDeckId;
        [SerializeField] private string player2DeckId = WeissBattleSetupOptions.DefaultSceneDeckId;

        public static GameManager Instance { get; private set; }
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
            _game = new WeissGame(BuildSetupOptions());
        }

        private async void Start()
        {
            try
            {
                WeissCardDatabase.EnsureInitialized();

                _game.StartGame();
                RefreshView();

                await _game.DealInitialHandsAsync();
                RefreshView();

                // 初期配札後の通常移動だけを汎用アニメーションへ流す
                _game.GameState.EventBus.Subscribe(BaseGameEvents.CardMoved, OnCardMoved);
                _game.GameState.EventBus.SubscribeToAll(_ => RefreshView());
                await _game.PerformMulliganPhase();
                RefreshView();
                await ExecuteCurrentTurnAsync();

                Debug.Log("Game Manager initialized successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Game Manager failed during initialization.");
            }
        }

        private void Update()
        {
            _game?.Update();
        }

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

        private void OnCardMoved(GameEvent evt)
        {
            if (evt.Data is CardMovedEventData data && GameView.Instance != null)
            {
                var weissCard = data.Card as WeissCard;
                if (weissCard != null)
                {
                    GameView.Instance.PrepareCardAnimation(weissCard);
                    GameView.Instance.StartCardAnimation(weissCard);
                }
            }
        }

        private WeissBattleSetupOptions BuildSetupOptions()
        {
            return new WeissBattleSetupOptions
            {
                Player1 = new WeissPlayerSetupOptions
                {
                    Name = player1Name,
                    ControlMode = player1ControlMode,
                    DeckId = string.IsNullOrWhiteSpace(player1DeckId)
                        ? WeissBattleSetupOptions.DefaultSceneDeckId
                        : player1DeckId
                },
                Player2 = new WeissPlayerSetupOptions
                {
                    Name = player2Name,
                    ControlMode = player2ControlMode,
                    DeckId = string.IsNullOrWhiteSpace(player2DeckId)
                        ? WeissBattleSetupOptions.DefaultSceneDeckId
                        : player2DeckId
                }
            };
        }
    }
}
