using System;
using System.Threading.Tasks;
using TCG.Weiss;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Weissカード1枚分の表示を担当するUIコンポーネントです。
    /// 表裏の切り替え、選択状態、レスト回転、カード詳細表示を扱います。
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Card Data")]
        private WeissCard _card;
        public WeissCard Card => _card;

        [Header("Sprites")]
        private Sprite _weissCardBackSprite;
        private Sprite _schwarzCardBackSprite;
        private Sprite _defaultCardSprite;

        [Header("UI Objects")]
        [SerializeField] private GameObject cardFrontObject;
        [SerializeField] private GameObject cardBackObject;
        [SerializeField] private Button cardButton;
        [SerializeField] private GameObject selectedOverlayObject;
        [SerializeField] private bool displayClimaxHorizontally;

        private Image _cardFrontImage;
        private Image _cardBackImage;

        private bool _isSelectionMode;
        private bool _isSelected;
        private bool _isSelectable = true;

        private readonly Color _normalTint = Color.white;
        private readonly Color _selectedTint = new Color(1f, 0.55f, 0.55f, 1f);
        private readonly Color _disabledTint = new Color(1f, 1f, 1f, 0.45f);
        private TaskCompletionSource<bool> _moveCompletionSource;
        private string _currentFrontImagePath;
        private int _frontImageLoadVersion;

        public bool IsSelected => _isSelected;
        public event Action<CardUI> CardClicked;

        [Header("Resource Paths")]
        [SerializeField] private string weissCardBackPath = "Images/WeissCardBack";
        [SerializeField] private string schwarzCardBackPath = "Images/SchwarzCardBack";
        [SerializeField] private string defaultCardImagePath = "Images/DefaultCard";

        private Coroutine _moveCoroutine;

        private void Awake()
        {
            _weissCardBackSprite = Resources.Load<Sprite>(weissCardBackPath);
            _schwarzCardBackSprite = Resources.Load<Sprite>(schwarzCardBackPath);
            _defaultCardSprite = Resources.Load<Sprite>(defaultCardImagePath);

            if (cardFrontObject != null) _cardFrontImage = cardFrontObject.GetComponent<Image>();
            if (cardBackObject != null) _cardBackImage = cardBackObject.GetComponent<Image>();

            if (_cardFrontImage != null) _cardFrontImage.preserveAspect = true;
            if (_cardBackImage != null) _cardBackImage.preserveAspect = true;

            cardButton?.onClick.AddListener(OnCardClicked);
            SetSelected(false);

            if (_cardFrontImage == null) Debug.LogError("CardUI front image is missing.", this);
            if (_cardBackImage == null) Debug.LogError("CardUI back image is missing.", this);
            if (_weissCardBackSprite == null) Debug.LogWarning($"Weiss card back sprite was not found at Resources/{weissCardBackPath}");
            if (_schwarzCardBackSprite == null) Debug.LogWarning($"Schwarz card back sprite was not found at Resources/{schwarzCardBackPath}");
            if (_defaultCardSprite == null) Debug.LogWarning($"Default card sprite was not found at Resources/{defaultCardImagePath}");
        }

        public void SetCard(WeissCard card)
        {
            _card = card;
            UpdateCardVisuals();
        }

        private void OnCardClicked()
        {
            if (_isSelectionMode && !_isSelectable)
            {
                return;
            }

            CardClicked?.Invoke(this);

            if (_isSelectionMode)
            {
                ToggleSelected();
                return;
            }

            if (_card != null && GameView.Instance != null)
            {
                GameView.Instance.ShowCardDetail(_card);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (cardButton == null && eventData.button == PointerEventData.InputButton.Left)
            {
                OnCardClicked();
            }
        }

        public void SetSelectionMode(bool enabled)
        {
            _isSelectionMode = enabled;
            if (!enabled)
            {
                SetSelected(false);
            }

            UpdateTint();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (selectedOverlayObject != null)
            {
                selectedOverlayObject.SetActive(selected);
            }

            UpdateTint();
        }

        /// <summary>
        /// 選択モード中にこのカードが選択候補かどうかを設定します。
        /// 候補外のカードは未選択へ戻し、ボタン操作を無効化して半透明表示にします。
        /// </summary>
        public void SetSelectable(bool selectable)
        {
            _isSelectable = selectable;
            if (!selectable)
            {
                _isSelected = false;
            }

            if (cardButton != null)
            {
                cardButton.interactable = !_isSelectionMode || selectable;
            }

            UpdateTint();
        }

        public void ToggleSelected()
        {
            SetSelected(!_isSelected);
        }

        public void SetDisplayClimaxHorizontally(bool enabled)
        {
            displayClimaxHorizontally = enabled;
            if (_card != null)
            {
                UpdateCardVisuals();
            }
        }

        private void UpdateCardVisuals()
        {
            if (_card == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            bool isFaceUp = _card.IsFaceUp;
            bool isClimax = IsClimaxCard(_card.Data);

            if (cardFrontObject != null) cardFrontObject.SetActive(isFaceUp);
            if (cardBackObject != null) cardBackObject.SetActive(!isFaceUp);

            if (isFaceUp)
            {
                UpdateFrontImage(_card.Data, isClimax);
            }
            else
            {
                UpdateBackImage();
            }

            UpdateCardRotation();
        }

        private void UpdateFrontImage(WeissCardData cardData, bool isClimax)
        {
            if (_cardFrontImage == null)
            {
                return;
            }
            ApplyCardImagePresentation(_cardFrontImage.rectTransform, isClimax, displayClimaxHorizontally);

            if (cardData == null || string.IsNullOrEmpty(cardData.ImagePath))
            {
                _currentFrontImagePath = null;
                _cardFrontImage.sprite = _defaultCardSprite;
                return;
            }

            string imagePath = cardData.ImagePath;
            if (imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // 非同期ロード中に同じCardUIが別カードへ再利用されることがあるため、
                // パスとバージョンを照合して古いロード結果を捨てます。
                if (ImageLoader.TryGetCachedSprite(imagePath, out var cachedSprite))
                {
                    _currentFrontImagePath = imagePath;
                    _cardFrontImage.sprite = cachedSprite;
                    return;
                }

                if (_currentFrontImagePath != imagePath || _cardFrontImage.sprite == null)
                {
                    _cardFrontImage.sprite = _defaultCardSprite;
                }

                _currentFrontImagePath = imagePath;
                int loadVersion = ++_frontImageLoadVersion;
                StartCoroutine(ImageLoader.LoadImage(imagePath, null, sprite =>
                {
                    if (this == null || _cardFrontImage == null)
                    {
                        return;
                    }

                    if (_currentFrontImagePath != imagePath || loadVersion != _frontImageLoadVersion)
                    {
                        return;
                    }

                    if (sprite != null)
                    {
                        _cardFrontImage.sprite = sprite;
                    }
                }));
                return;
            }

            _currentFrontImagePath = imagePath;
            Sprite spriteToDisplay = Resources.Load<Sprite>(imagePath);
            if (spriteToDisplay != null)
            {
                _cardFrontImage.sprite = spriteToDisplay;
            }
            else
            {
                _cardFrontImage.sprite = _defaultCardSprite;
            }
        }

        private void UpdateBackImage()
        {
            if (_cardBackImage != null)
            {
                _cardBackImage.sprite = GetCardBackSprite();
            }
        }

        private void UpdateCardRotation()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            float zRotation = _card.IsRested ? -90f : 0f;
            rect.localRotation = Quaternion.Euler(0, 0, zRotation);
        }

        public static bool IsClimaxCard(WeissCardData cardData)
        {
            return cardData != null &&
                   (cardData.CardType == "Climax" || cardData.CardType == "クライマックス");
        }

        public static void ApplyCardImagePresentation(RectTransform rect, bool isClimax, bool displayHorizontally = false)
        {
            if (rect == null)
            {
                return;
            }

            Vector2 currentSize = rect.rect.size;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            if (currentSize.x > 0f && currentSize.y > 0f)
            {
                rect.sizeDelta = currentSize;
            }
            rect.anchoredPosition = Vector2.zero;

            bool rotateClimaxToPortrait = isClimax && !displayHorizontally;
            rect.localRotation = rotateClimaxToPortrait ? Quaternion.Euler(0, 0, -90f) : Quaternion.identity;

            float scale = isClimax ? 1.4f : 1f;
            rect.localScale = new Vector3(scale, scale, 1f);
        }

        private Sprite GetCardBackSprite()
        {
            if (_card != null && _card.Data != null)
            {
                string side = _card.Data.Side;
                if (side == "Weiss" || side == "ヴァイス") return _weissCardBackSprite;
                if (side == "Schwarz" || side == "シュヴァルツ") return _schwarzCardBackSprite;
            }

            return _weissCardBackSprite;
        }

        private void UpdateTint()
        {
            var tint = _isSelected
                ? _selectedTint
                : (_isSelectionMode && !_isSelectable ? _disabledTint : _normalTint);

            if (_cardFrontImage != null) _cardFrontImage.color = tint;
            if (_cardBackImage != null) _cardBackImage.color = tint;
        }

        public void MoveToPosition(Vector3 targetPosition, float duration = 0.3f)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, duration));
        }

        /// <summary>
        /// MoveToPositionと同じ補間移動をTaskとして待機できる形で開始します。
        /// 初期配札など、複数段のカード移動を順番に再生したい処理で使用します。
        /// </summary>
        public Task MoveToPositionAsync(Vector3 targetPosition, float duration = 0.3f)
        {
            _moveCompletionSource?.TrySetCanceled();
            _moveCompletionSource = new TaskCompletionSource<bool>();
            MoveToPosition(targetPosition, duration);
            return _moveCompletionSource.Task;
        }

        private System.Collections.IEnumerator MoveCoroutine(Vector3 targetPosition, float duration)
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) yield break;

            Vector3 startPosition = rect.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = 1f - (1f - t) * (1f - t);
                rect.position = Vector3.Lerp(startPosition, targetPosition, easeT);
                yield return null;
            }

            rect.position = targetPosition;
            _moveCoroutine = null;
            _moveCompletionSource?.TrySetResult(true);
            _moveCompletionSource = null;
        }
    }
}
