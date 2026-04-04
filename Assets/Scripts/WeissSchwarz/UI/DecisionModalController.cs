using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲーム中の入力要求を中央モーダルで受けるための単一コントローラー。
    /// まずはマリガン入力を対象に実装し、今後は他の選択UIもここへ集約する。
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

        private TaskCompletionSource<List<WeissCard>> _mulliganCompletionSource;
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
            CancelActiveRequest();

            _activeHandZone = handZoneUI;
            _activeHandZone?.EnterMulliganSelectionMode();
            _mulliganCompletionSource = new TaskCompletionSource<List<WeissCard>>();

            if (titleText != null) titleText.text = "マリガン選択";
            if (messageText != null) messageText.text = $"{player?.Name ?? "Player"}: 交換したいカードを選んでください";
            if (primaryButtonText != null) primaryButtonText.text = "決定";
            if (secondaryButtonText != null) secondaryButtonText.text = "スキップ";

            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveAllListeners();
                primaryButton.onClick.AddListener(ConfirmMulligan);
            }

            if (secondaryButton != null)
            {
                secondaryButton.onClick.RemoveAllListeners();
                secondaryButton.onClick.AddListener(CancelMulligan);
            }

            SetTertiaryButton(null, null);

            SetVisible(true);
            return _mulliganCompletionSource.Task;
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
        /// tertiaryButtonが未設定の場合は「能力起動」を出せないため、EndPhaseを返します。
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

        private void ConfirmMulligan()
        {
            var selected = _activeHandZone != null
                ? _activeHandZone.GetSelectedCardsForMulligan()
                : new List<WeissCard>();
            CompleteMulligan(selected);
        }

        private void CancelMulligan()
        {
            CompleteMulligan(new List<WeissCard>());
        }

        private void CompleteMulligan(List<WeissCard> selectedCards)
        {
            _activeHandZone?.ExitMulliganSelectionMode();
            _activeHandZone = null;

            SetVisible(false);

            ClearAllButtonListeners();

            if (_mulliganCompletionSource != null && !_mulliganCompletionSource.Task.IsCompleted)
            {
                _mulliganCompletionSource.SetResult(selectedCards ?? new List<WeissCard>());
            }

            _mulliganCompletionSource = null;
        }

        private void CompleteYesNo(bool choice)
        {
            _activeHandZone?.ExitMulliganSelectionMode();
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
            _activeHandZone?.ExitMulliganSelectionMode();
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
            if (_mulliganCompletionSource != null && !_mulliganCompletionSource.Task.IsCompleted)
            {
                CompleteMulligan(new List<WeissCard>());
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
            SetTertiaryButton(null, null);
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
