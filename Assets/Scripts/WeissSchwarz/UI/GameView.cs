using TCG.Core;
using TCG.Weiss; // For IHandZone, IStageZone, etc.
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Acts as the main controller for updating the entire game UI for a player.
    /// It retrieves game state from the core logic and passes it to the specialized UI zone managers.
    /// </summary>
    public class GameView : MonoBehaviour
    {
        /// <summary>
        /// GameViewのシングルトンインスタンス。
        /// </summary>
        public static GameView Instance { get; private set; }

        [Header("Player UI Zone Managers")]
        [SerializeField] private HandZoneUI handZoneUI;
        [SerializeField] private StageZoneUI stageZoneUI;
        [SerializeField] private ClimaxZoneUI climaxZoneUI;
        [SerializeField] private LevelZoneUI levelZoneUI;
        [SerializeField] private StockZoneUI stockZoneUI;
        [SerializeField] private ClockZoneUI clockZoneUI;
        [SerializeField] private WaitingRoomUI waitingRoomUI;
        [SerializeField] private DeckZoneUI deckZoneUI;
        [SerializeField] private MemoryZoneUI memoryZoneUI;

        [Header("Card Detail View")]
        [SerializeField] private CardDetailView cardDetailView;

        [Header("Interaction Buttons")]
        [SerializeField] private GameObject mulliganConfirmButton;

        private UIGamePlayerController _activeController;

        /// <summary>
        /// UnityのAwakeイベントで呼び出されます。
        /// GameViewのシングルトンインスタンスを設定し、UI要素の初期状態を準備します。
        /// </summary>
        private void Awake()
        {
            // シングルトンパターンの実装
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            // マリガン確認ボタンは初期状態で非アクティブに設定
            mulliganConfirmButton?.SetActive(false);
            // カード詳細ビューも初期状態で非表示に設定
            cardDetailView?.Hide(); 
        }

        /// <summary>
        /// 指定されたカードの詳細情報をUIに表示します。
        /// </summary>
        /// <param name="card">詳細を表示するヴァイスシュヴァルツのカード。</param>
        public void ShowCardDetail(WeissCard card)
        {
            cardDetailView?.Show(card);
        }

        /// <summary>
        /// カード詳細ビューを非表示にします。
        /// </summary>
        public void HideCardDetail()
        {
            cardDetailView?.Hide();
        }

        /// <summary>
        /// マリガン選択フェーズを開始し、UIを更新します。
        /// </summary>
        /// <param name="controller">マリガンを行うプレイヤーのUIコントローラー。</param>
        public void BeginMulliganSelection(UIGamePlayerController controller)
        {
            _activeController = controller; // 現在のマリガン操作を担当するコントローラーを保持
            handZoneUI.EnterMulliganSelectionMode(); // 手札UIをマリガン選択モードに切り替える
            mulliganConfirmButton?.SetActive(true); // マリガン確定ボタンを表示
            Debug.Log("GameView: Mulligan selection started. Confirm button is now visible.");
        }

        /// <summary>
        /// マリガン確定ボタンがクリックされたときに呼び出されます。
        /// 選択されたカードをコントローラーに渡し、UIをリセットします。
        /// </summary>
        public void ConfirmMulliganClicked()
        {
            if (_activeController == null) return; // コントローラーが設定されていなければ何もしない

            // 手札UIからマリガンで選択されたカードのリストを取得
            var selectedCards = handZoneUI.GetSelectedCardsForMulligan();
            // 取得したカードリストをアクティブなコントローラーに渡し、マリガン処理を実行
            _activeController.ConfirmMulligan(selectedCards);

            // UI要素をクリーンアップ
            mulliganConfirmButton?.SetActive(false); // マリガン確定ボタンを非表示
            _activeController = null; // アクティブコントローラーをクリア
            Debug.Log("GameView: Mulligan confirmed. UI has been reset.");
        }

        /// <summary>
        /// 管理対象の全てのUIゾーンを更新し、指定されたプレイヤーのゲーム状態を反映させます。
        /// </summary>
        /// <param name="player">状態を表示するプレイヤー。</param>
        public void UpdateView(Player player)
        {
            if (player == null)
            {
                Debug.LogError("Cannot update view for a null player.");
                return;
            }

            // 各ゾーンUIマネージャーを更新
            // 各UIゾーンが対象のプレイヤーの対応するゾーンデータと同期されるようにします。
            if (handZoneUI != null)
            {
                var handZone = player.GetZone<IHandZone<WeissCard>>() as HandZone;
                handZoneUI.UpdateZone(handZone);
            }

            if (stageZoneUI != null)
            {
                var stageZone = player.GetZone<IStageZone<WeissCard>>() as StageZone;
                stageZoneUI.UpdateZone(stageZone);
            }

            if (climaxZoneUI != null)
            {
                var climaxZone = player.GetZone<IClimaxZone<WeissCard>>() as ClimaxZone;
                climaxZoneUI.UpdateZone(climaxZone);
            }

            if (stockZoneUI != null)
            {
                var stockZone = player.GetZone<IStockZone<WeissCard>>() as StockZone;
                stockZoneUI.UpdateZone(stockZone);
            }

            if (clockZoneUI != null)
            {
                var clockZone = player.GetZone<IClockZone<WeissCard>>() as ClockZone;
                clockZoneUI.UpdateZone(clockZone);
            }

            if (waitingRoomUI != null)
            {
                var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>() as WaitingRoomZone;
                waitingRoomUI.UpdateZone(waitingRoom);
            }

            if (deckZoneUI != null)
            {
                var deckZone = player.GetZone<IDeckZone<WeissCard>>() as DeckZone;
                deckZoneUI.UpdateZone(deckZone);
            }
            if (levelZoneUI != null)
            {
                var levelZone = player.GetZone<ILevelZone<WeissCard>>() as LevelZone;
                levelZoneUI.UpdateZone(levelZone);
            }
            if (levelZoneUI != null)
            {
                var levelZone = player.GetZone<ILevelZone<WeissCard>>() as LevelZone;
                levelZoneUI.UpdateZone(levelZone);
            }
        }
    }
}
