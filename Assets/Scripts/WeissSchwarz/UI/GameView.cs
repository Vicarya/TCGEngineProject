using System.Collections.Generic;
using System.Threading.Tasks;
using TCG.Core;
using TCG.Weiss;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 現在表示しているプレイヤー盤面のUI更新と入力要求をまとめるコーディネーター。
    /// ゲームロジックから直接各UI部品を触らせず、手札選択やモーダル表示、カード移動演出の入口をここに集約します。
    /// </summary>
    public class GameView : MonoBehaviour
    {
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

        [Header("Decision Modal")]
        [SerializeField] private DecisionModalController decisionModalController;

        [Header("Turn Player Marker")]
        [SerializeField] private TMP_Text turnPlayerMarkerText;
        [SerializeField] private TMP_FontAsset turnPlayerMarkerFont;
        [SerializeField] private string turnPlayerMarkerFormat = "TURN PLAYER\n{0}\nTURN {1}";

        private readonly Dictionary<string, Vector3> _cardOldPositions = new Dictionary<string, Vector3>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureTurnPlayerMarker();
            cardDetailView?.Hide();
        }

        public void ShowCardDetail(WeissCard card)
        {
            cardDetailView?.Show(card);
        }

        public void HideCardDetail()
        {
            cardDetailView?.Hide();
        }

        public Task<List<WeissCard>> RequestMulliganAsync(WeissPlayer player)
        {
            if (decisionModalController == null)
            {
                Debug.LogWarning("GameView: decisionModalController is not assigned. Mulligan will be skipped.");
                return Task.FromResult(new List<WeissCard>());
            }

            return decisionModalController.ShowMulliganAsync(player, handZoneUI);
        }

        /// <summary>
        /// 手札から1枚を選ばせる入力要求をDecisionModalControllerへ委譲します。
        /// optionalがtrueの場合は0枚選択を許可し、falseの場合は1枚選ぶまで確定できない設定にします。
        /// </summary>
        public async Task<WeissCard> RequestHandCardSelectionAsync(
            WeissPlayer player,
            IReadOnlyList<WeissCard> selectableCards,
            bool optional,
            string title,
            string message)
        {
            if (decisionModalController == null || handZoneUI == null)
            {
                Debug.LogWarning("GameView: Hand card selection UI is not fully assigned. Selection will be skipped.");
                return null;
            }

            int minSelection = optional ? 0 : 1;
            var selectedCards = await decisionModalController.ShowHandCardSelectionAsync(
                player,
                handZoneUI,
                selectableCards,
                minSelection,
                maxSelection: 1,
                allowCancel: optional,
                title,
                message,
                confirmLabel: "決定",
                cancelLabel: optional ? "キャンセル" : string.Empty);

            return selectedCards.Count > 0 ? selectedCards[0] : null;
        }

        public Task<bool> RequestYesNoAsync(string title, string question, bool defaultValue = false)
        {
            if (decisionModalController == null)
            {
                Debug.LogWarning("GameView: decisionModalController is not assigned. Yes/No fallback will be used.");
                return Task.FromResult(defaultValue);
            }

            return decisionModalController.ShowYesNoAsync(title, question);
        }

        public Task<MainPhaseAction> RequestMainPhaseActionAsync()
        {
            if (decisionModalController == null)
            {
                Debug.LogWarning("GameView: decisionModalController is not assigned. EndPhase will be returned.");
                return Task.FromResult(MainPhaseAction.EndPhase);
            }

            return decisionModalController.ShowMainPhaseActionAsync();
        }

        /// <summary>
        /// 初期配札用に、山札位置から一度見切れ位置へ移動して表向きにし、最終的な手札位置へ戻す演出を行います。
        /// 通常のCardMovedイベントではなく、DealInitialHandsAsyncから逐次awaitされるため、配札順の演出を保てます。
        /// </summary>
        public async Task AnimateInitialHandDealAsync(WeissPlayer player, WeissCard card)
        {
            if (player == null || card == null || handZoneUI == null || deckZoneUI == null)
            {
                return;
            }

            UpdateView(player);
            await WaitForEndOfFrameAsync();

            if (!deckZoneUI.TryGetDeckCardWorldPosition(out var sourcePosition))
            {
                return;
            }

            if (!handZoneUI.TryGetCardUI(card, out var cardUI) || cardUI == null)
            {
                return;
            }

            var targetPosition = cardUI.transform.position;
            var revealPosition = handZoneUI.GetDealRevealWorldPosition();

            card.IsFaceUp = false;
            cardUI.SetCard(card);
            cardUI.transform.position = sourcePosition;

            await cardUI.MoveToPositionAsync(revealPosition, 0.22f);

            card.IsFaceUp = true;
            cardUI.SetCard(card);
            await WaitForSecondsAsync(0.06f);

            await cardUI.MoveToPositionAsync(targetPosition, 0.24f);
        }

        public void UpdateView(Player player)
        {
            if (player == null)
            {
                Debug.LogError("Cannot update view for a null player.");
                return;
            }

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

            if (memoryZoneUI != null)
            {
                var memoryZone = player.GetZone<IMemoryZone<WeissCard>>();
                memoryZoneUI.UpdateZone(memoryZone);
            }

            UpdateTurnPlayerMarker(player);
        }

        public void PrepareCardAnimation(WeissCard card)
        {
            if (card == null || card.Data == null)
            {
                return;
            }

            var allCardUIs = GetComponentsInChildren<CardUI>(true);
            foreach (var ui in allCardUIs)
            {
                if (ui.Card == card)
                {
                    string key = card.Data.CardCode + "_" + card.GetHashCode();
                    _cardOldPositions[key] = ui.transform.position;
                    break;
                }
            }
        }

        public void StartCardAnimation(WeissCard card)
        {
            if (card == null || card.Data == null)
            {
                return;
            }

            StartCoroutine(StartCardAnimationRoutine(card));
        }

        private System.Collections.IEnumerator StartCardAnimationRoutine(WeissCard card)
        {
            yield return new WaitForEndOfFrame();

            string key = card.Data.CardCode + "_" + card.GetHashCode();
            if (_cardOldPositions.TryGetValue(key, out var oldPos))
            {
                _cardOldPositions.Remove(key);
                var allCardUIs = GetComponentsInChildren<CardUI>(true);
                foreach (var ui in allCardUIs)
                {
                    if (ui.Card == card)
                    {
                        Vector3 targetPos = ui.transform.position;
                        ui.transform.position = oldPos;
                        ui.MoveToPosition(targetPos, 0.3f);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Unityのフレーム更新完了をTaskとして待機するための薄い変換です。
        /// UIレイアウト反映後の座標を取得したい処理で使用します。
        /// </summary>
        private Task WaitForEndOfFrameAsync()
        {
            var completionSource = new TaskCompletionSource<bool>();
            StartCoroutine(WaitForEndOfFrameRoutine(completionSource));
            return completionSource.Task;
        }

        private Task WaitForSecondsAsync(float seconds)
        {
            var completionSource = new TaskCompletionSource<bool>();
            StartCoroutine(WaitForSecondsRoutine(seconds, completionSource));
            return completionSource.Task;
        }

        private void UpdateTurnPlayerMarker(Player player)
        {
            if (turnPlayerMarkerText == null || player == null)
            {
                return;
            }

            int turnCounter = 0;
            if (GameManager.Instance?.Game?.GameState != null)
            {
                turnCounter = GameManager.Instance.Game.GameState.TurnCounter;
            }

            string playerName = string.IsNullOrWhiteSpace(player.Name) ? "Unknown" : player.Name;
            turnPlayerMarkerText.text = string.Format(turnPlayerMarkerFormat, playerName, turnCounter);
        }

        private void EnsureTurnPlayerMarker()
        {
            if (turnPlayerMarkerText != null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                return;
            }

            var markerObject = new GameObject(
                "TurnPlayerMarker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            markerObject.transform.SetParent(canvas.transform, false);

            var markerRect = markerObject.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0f, 1f);
            markerRect.anchorMax = new Vector2(0f, 1f);
            markerRect.pivot = new Vector2(0f, 1f);
            markerRect.anchoredPosition = new Vector2(16f, -16f);
            markerRect.sizeDelta = new Vector2(220f, 84f);

            var background = markerObject.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);

            var textObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(markerObject.transform, false);

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            turnPlayerMarkerText = textObject.GetComponent<TextMeshProUGUI>();
            turnPlayerMarkerText.font = ResolveTurnPlayerMarkerFont();

            turnPlayerMarkerText.margin = new Vector4(12f, 8f, 12f, 8f);
            turnPlayerMarkerText.fontSize = 24f;
            turnPlayerMarkerText.alignment = TextAlignmentOptions.TopLeft;
            turnPlayerMarkerText.color = Color.white;
            turnPlayerMarkerText.text = "TURN PLAYER";
        }

        private TMP_FontAsset ResolveTurnPlayerMarkerFont()
        {
            if (turnPlayerMarkerFont != null)
            {
                return turnPlayerMarkerFont;
            }

            var notoFont = FindFontByName(TMP_Settings.fallbackFontAssets);
            if (notoFont != null)
            {
                return notoFont;
            }

            notoFont = FindFontByName(Resources.FindObjectsOfTypeAll<TMP_FontAsset>());
            if (notoFont != null)
            {
                return notoFont;
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static TMP_FontAsset FindFontByName(IEnumerable<TMP_FontAsset> fonts)
        {
            if (fonts == null)
            {
                return null;
            }

            foreach (var font in fonts)
            {
                if (font == null)
                {
                    continue;
                }

                string assetName = font.name ?? string.Empty;
                string familyName = font.faceInfo.familyName ?? string.Empty;
                if (assetName.Contains("NotoSansJP", System.StringComparison.OrdinalIgnoreCase) ||
                    assetName.Contains("Noto Sans JP", System.StringComparison.OrdinalIgnoreCase) ||
                    familyName.Contains("Noto Sans JP", System.StringComparison.OrdinalIgnoreCase))
                {
                    return font;
                }
            }

            return null;
        }

        private static System.Collections.IEnumerator WaitForEndOfFrameRoutine(TaskCompletionSource<bool> completionSource)
        {
            yield return new WaitForEndOfFrame();
            completionSource.TrySetResult(true);
        }

        private static System.Collections.IEnumerator WaitForSecondsRoutine(float seconds, TaskCompletionSource<bool> completionSource)
        {
            yield return new WaitForSeconds(seconds);
            completionSource.TrySetResult(true);
        }
    }
}
