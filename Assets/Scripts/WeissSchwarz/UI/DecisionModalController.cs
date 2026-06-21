using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using TCG.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲーム中の入力要求を中央モーダルで受けるための単一コントローラー。
    /// マリガン、手札選択、Yes/No、メインフェイズ行動選択を同じ表示基盤へ集約します。
    /// </summary>
    public class DecisionModalController : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject modalRoot;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text primaryButtonText;
        [SerializeField] private TMP_Text secondaryButtonText;
        [SerializeField] private TMP_Text tertiaryButtonText;

        [Header("Buttons")]
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private Button tertiaryButton;

        private TaskCompletionSource<bool> _yesNoCompletionSource;
        private TaskCompletionSource<MainPhaseAction> _mainPhaseActionCompletionSource;
        private HandZoneUI _activeHandZone;

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnDisable()
        {
            CancelActiveRequest();
        }

        /// <summary>
        /// マリガン選択モーダルを表示し、確定またはキャンセルまで待機します。
        /// </summary>
        public Task<List<WeissCard>> ShowMulliganAsync(WeissPlayer player, HandZoneUI handZoneUI)
        {
            var handCards = player?.GetZone<IHandZone<WeissCard>>()?.Cards ?? new List<WeissCard>();
            return ShowHandCardSelectionAsync(
                player,
                handZoneUI,
                handCards,
                minSelection: 0,
                maxSelection: handCards.Count,
                allowCancel: true,
                title: "マリガン選択",
                message: $"{player?.Name ?? "Player"}: 交換したいカードを選んでください",
                confirmLabel: "決定",
                cancelLabel: "スキップ");
        }

        /// <summary>
        /// 手札カードの選択モーダルを表示し、HandZoneUI側の選択セッション完了まで待機します。
        /// selectableCardsで選べるカードを制限し、minSelection/maxSelectionで必須枚数と上限枚数を制御します。
        /// allowCancelがfalseの場合はキャンセルボタンを非表示にし、必要枚数を満たすまで決定できません。
        /// </summary>
        public Task<List<WeissCard>> ShowHandCardSelectionAsync(
            WeissPlayer player,
            HandZoneUI handZoneUI,
            IReadOnlyList<WeissCard> selectableCards,
            int minSelection,
            int maxSelection,
            bool allowCancel,
            string title,
            string message,
            string confirmLabel,
            string cancelLabel)
        {
            CancelActiveRequest();

            _activeHandZone = handZoneUI;
            var selectionTask = _activeHandZone != null
                ? _activeHandZone.BeginCardSelectionAsync(selectableCards, minSelection, maxSelection, allowCancel)
                : Task.FromResult(new List<WeissCard>());

            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;

            ConfigureButton(primaryButton, primaryButtonText, confirmLabel, ConfirmHandSelection, true);
            ConfigureButton(secondaryButton, secondaryButtonText, cancelLabel, CancelHandSelection, allowCancel);
            SetTertiaryButton(null, null);

            SetVisible(true);
            return selectionTask;
        }

        /// <summary>
        /// Yes/Noを選択する2択モーダルを表示します。
        /// </summary>
        public Task<bool> ShowYesNoAsync(string title, string question, string yesLabel = "はい", string noLabel = "いいえ")
        {
            CancelActiveRequest();
            _yesNoCompletionSource = new TaskCompletionSource<bool>();

            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = question;
            if (primaryButtonText != null) primaryButtonText.text = yesLabel;
            if (secondaryButtonText != null) secondaryButtonText.text = noLabel;

            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveAllListeners();
                primaryButton.onClick.AddListener(() => CompleteYesNo(true));
            }

            if (secondaryButton != null)
            {
                secondaryButton.onClick.RemoveAllListeners();
                secondaryButton.onClick.AddListener(() => CompleteYesNo(false));
            }

            SetTertiaryButton(null, null);
            SetVisible(true);
            return _yesNoCompletionSource.Task;
        }

        /// <summary>
        /// メインフェイズ行動の3択モーダルを表示します。
        /// tertiaryButtonが未設定の場合は「能力起動」ボタンだけを非表示にし、カードプレイとフェイズ終了は利用できます。
        /// </summary>
        public Task<MainPhaseAction> ShowMainPhaseActionAsync(string title = "メインフェイズ", string message = "行動を選択してください")
        {
            CancelActiveRequest();
            _mainPhaseActionCompletionSource = new TaskCompletionSource<MainPhaseAction>();

            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            if (primaryButtonText != null) primaryButtonText.text = "カードプレイ";
            if (secondaryButtonText != null) secondaryButtonText.text = "フェイズ終了";

            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveAllListeners();
                primaryButton.onClick.AddListener(() => CompleteMainPhaseAction(MainPhaseAction.PlayCard));
            }

            if (secondaryButton != null)
            {
                secondaryButton.onClick.RemoveAllListeners();
                secondaryButton.onClick.AddListener(() => CompleteMainPhaseAction(MainPhaseAction.EndPhase));
            }

            if (tertiaryButton != null)
            {
                SetTertiaryButton("能力起動", () => CompleteMainPhaseAction(MainPhaseAction.UseAbility));
            }
            else
            {
                Debug.LogWarning("DecisionModalController: tertiaryButton is not assigned. UseAbility option is unavailable.");
                SetTertiaryButton(null, null);
            }

            SetVisible(true);
            return _mainPhaseActionCompletionSource.Task;
        }

        private void ConfirmHandSelection()
        {
            if (_activeHandZone != null && !_activeHandZone.TryConfirmSelection())
            {
                return;
            }

            CloseHandSelectionModal();
        }

        private void CancelHandSelection()
        {
            if (_activeHandZone != null && !_activeHandZone.TryCancelSelection())
            {
                return;
            }

            CloseHandSelectionModal();
        }

        private void CompleteYesNo(bool choice)
        {
            _activeHandZone = null;

            SetVisible(false);
            ClearAllButtonListeners();

            if (_yesNoCompletionSource != null && !_yesNoCompletionSource.Task.IsCompleted)
            {
                _yesNoCompletionSource.SetResult(choice);
            }

            _yesNoCompletionSource = null;
        }

        private void CompleteMainPhaseAction(MainPhaseAction action)
        {
            _activeHandZone = null;

            SetVisible(false);
            ClearAllButtonListeners();

            if (_mainPhaseActionCompletionSource != null && !_mainPhaseActionCompletionSource.Task.IsCompleted)
            {
                _mainPhaseActionCompletionSource.SetResult(action);
            }

            _mainPhaseActionCompletionSource = null;
        }

        private void CancelActiveRequest()
        {
            if (_activeHandZone != null)
            {
                _activeHandZone.AbortSelection();
                CloseHandSelectionModal();
            }

            if (_yesNoCompletionSource != null && !_yesNoCompletionSource.Task.IsCompleted)
            {
                CompleteYesNo(false);
            }

            if (_mainPhaseActionCompletionSource != null && !_mainPhaseActionCompletionSource.Task.IsCompleted)
            {
                CompleteMainPhaseAction(MainPhaseAction.EndPhase);
            }
        }

        private void ClearAllButtonListeners()
        {
            if (primaryButton != null) primaryButton.onClick.RemoveAllListeners();
            if (secondaryButton != null) secondaryButton.onClick.RemoveAllListeners();
            if (tertiaryButton != null) tertiaryButton.onClick.RemoveAllListeners();
            if (secondaryButton != null) secondaryButton.gameObject.SetActive(true);
            SetTertiaryButton(null, null);
        }

        private void CloseHandSelectionModal()
        {
            _activeHandZone = null;
            SetVisible(false);
            ClearAllButtonListeners();
        }

        private void ConfigureButton(
            Button button,
            TMP_Text labelText,
            string label,
            UnityEngine.Events.UnityAction onClick,
            bool visible)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(visible);
            if (labelText != null)
            {
                labelText.text = visible ? label : string.Empty;
            }

            if (visible && onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        private void SetTertiaryButton(string label, UnityEngine.Events.UnityAction onClick)
        {
            if (tertiaryButton == null)
            {
                return;
            }

            tertiaryButton.onClick.RemoveAllListeners();
            bool enabled = !string.IsNullOrEmpty(label) && onClick != null;
            tertiaryButton.gameObject.SetActive(enabled);
            if (tertiaryButtonText != null)
            {
                tertiaryButtonText.text = enabled ? label : string.Empty;
            }

            if (enabled)
            {
                tertiaryButton.onClick.AddListener(onClick);
            }
        }

        private void SetVisible(bool isVisible)
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(isVisible);
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
