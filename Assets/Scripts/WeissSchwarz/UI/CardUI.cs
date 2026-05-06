using System;
using TCG.Weiss;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲームフィールド上に表示される単一のヴァイスシュヴァルツカードの視覚的な表現を管理するMonoBehaviourクラス。
    /// カードの表/裏、レスト状態、クリックへの応答などを制御します。
    /// 表と裏のGameObjectを分けることで、アニメーションなどを実装しやすい階層構造を採用しています。
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Card Data")]
        private WeissCard _card;

        /// <summary>
        /// 現在このUIに関連付けられているカードのロジックデータ。
        /// </summary>
        public WeissCard Card => _card;

        [Header("Sprites")]
        private Sprite _weissCardBackSprite;
        private Sprite _schwarzCardBackSprite;
        private Sprite _defaultCardSprite;

        [Header("UI Objects")]
        [Tooltip("カードの表面を表示するGameObject。")]
        [SerializeField] private GameObject cardFrontObject;
        [Tooltip("カードの裏面を表示するGameObject。")]
        [SerializeField] private GameObject cardBackObject;
        [Tooltip("クリック操作を受け取るためのButtonコンポーネント。")]
        [SerializeField] private Button cardButton;
        [Tooltip("カードが選択されている時に表示するオーバーレイオブジェクト。")]
        [SerializeField] private GameObject selectedOverlayObject;

        private Image _cardFrontImage;
        private Image _cardBackImage;

        private bool _isSelectionMode;
        private bool _isSelected;

        private readonly Color _normalTint = Color.white;
        private readonly Color _selectedTint = new Color(1f, 0.55f, 0.55f, 1f);

        /// <summary>
        /// 現在カードが選択されているかどうか。
        /// </summary>
        public bool IsSelected => _isSelected;

        /// <summary>
        /// カードがクリックされたときに発行されるイベント。
        /// </summary>
        public event Action<CardUI> CardClicked;

        [Header("リソースパス")]
        [SerializeField] private string weissCardBackPath = "Images/WeissCardBack"; // ヴァイスサイドのカード裏面画像パス
        [SerializeField] private string schwarzCardBackPath = "Images/SchwarzCardBack"; // シュヴァルツサイドのカード裏面画像パス
        [SerializeField] private string defaultCardImagePath = "Images/DefaultCard"; // デフォルトのカード画像パス

        private void Awake()
        {
            // Resourcesフォルダからスプライトをロード
            _weissCardBackSprite = Resources.Load<Sprite>(weissCardBackPath);
            _schwarzCardBackSprite = Resources.Load<Sprite>(schwarzCardBackPath);
            _defaultCardSprite = Resources.Load<Sprite>(defaultCardImagePath);

            // 子オブジェクトからImageコンポーネントを取得
            if (cardFrontObject != null) _cardFrontImage = cardFrontObject.GetComponent<Image>();
            if (cardBackObject != null) _cardBackImage = cardBackObject.GetComponent<Image>();

            // ボタンクリックリスナーを追加
            if (_cardFrontImage != null) _cardFrontImage.preserveAspect = true;
            if (_cardBackImage != null) _cardBackImage.preserveAspect = true;
            cardButton?.onClick.AddListener(OnCardClicked);
            SetSelected(false);

            // 必要なコンポーネントやリソースが見つからない場合の警告（デバッグ用）
            if (_cardFrontImage == null) Debug.LogError("カード表面のGameObjectにImageコンポーネントが見つかりません。", this);
            if (_cardBackImage == null) Debug.LogError("カード裏面のGameObjectにImageコンポーネントが見つかりません。", this);
            if (_weissCardBackSprite == null) Debug.LogWarning($"ヴァイスのカード裏面スプライトが'Resources/{weissCardBackPath}'に見つかりません。");
            if (_schwarzCardBackSprite == null) Debug.LogWarning($"シュヴァルツのカード裏面スプライトが'Resources/{schwarzCardBackPath}'に見つかりません。");
            if (_defaultCardSprite == null) Debug.LogWarning($"デフォルトカードスプライトが'Resources/{defaultCardImagePath}'に見つかりません。");
        }

        /// <summary>
        /// 表示するカードデータを設定し、ビジュアルを最新の状態に更新します。
        /// </summary>
        /// <param name="card">表示対象のWeissCardインスタンス。</param>
        public void SetCard(WeissCard card)
        {
            _card = card;
            UpdateCardVisuals();
        }

        /// <summary>
        /// ボタンがクリックされた際の内部処理。
        /// 選択モード中であれば選択状態をトグルし、通常時はカード詳細を表示します。
        /// </summary>
        private void OnCardClicked()
        {
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
        
        /// <summary>
        /// Button未割り当てでもクリックを拾えるようにするフォールバック。
        /// </summary>
        /// <param name="eventData">クリックイベントのデータ。</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (cardButton == null && eventData.button == PointerEventData.InputButton.Left)
            {
                OnCardClicked();
            }
        }

        /// <summary>
        /// 選択モード（マリガン時など）を有効または無効にします。
        /// 無効にする際、選択状態は解除されます。
        /// </summary>
        /// <param name="enabled">有効にする場合はtrue。</param>
        public void SetSelectionMode(bool enabled)
        {
            _isSelectionMode = enabled;
            if (!enabled)
            {
                SetSelected(false);
            }
        }

        /// <summary>
        /// カードの選択状態を明示的に設定します。
        /// </summary>
        /// <param name="selected">選択状態にする場合はtrue。</param>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (selectedOverlayObject != null)
            {
                selectedOverlayObject.SetActive(selected);
            }

            var tint = selected ? _selectedTint : _normalTint;
            if (_cardFrontImage != null) _cardFrontImage.color = tint;
            if (_cardBackImage != null) _cardBackImage.color = tint;
        }

        /// <summary>
        /// カードの選択状態を反転させます。
        /// </summary>
        public void ToggleSelected()
        {
            SetSelected(!_isSelected);
        }

        /// <summary>
        /// カードの表裏、回転、画像などのビジュアル要素を現在のデータに基づいて更新します。
        /// </summary>
        private void UpdateCardVisuals()
        {
            if (_card == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            bool isFaceUp = _card.IsFaceUp;
            if (cardFrontObject != null) cardFrontObject.SetActive(isFaceUp);
            if (cardBackObject != null) cardBackObject.SetActive(!isFaceUp);

            if (isFaceUp)
            {
                Sprite spriteToDisplay = null;
                var cardData = _card.Data;

                if (cardData != null && !string.IsNullOrEmpty(cardData.ImagePath))
                {
                    spriteToDisplay = Resources.Load<Sprite>(cardData.ImagePath);
                }

                if (spriteToDisplay == null)
                {
                    spriteToDisplay = _defaultCardSprite; // 画像が見つからない場合はデフォルト画像
                }

                if (_cardFrontImage != null)
                {
                    _cardFrontImage.sprite = spriteToDisplay;
                }
            }
            else
            {
                // 裏向きの場合、カードの裏面画像を表示（サイドによって切り替え）
                if (_cardBackImage != null)
                {
                    _cardBackImage.sprite = GetCardBackSprite();
                }
            }

            // 回転状態の更新
            float zRotation = 0f;
            bool isClimax = _card.Data != null &&
                            (_card.Data.CardType == "Climax" || _card.Data.CardType == "クライマックス");

            // レスト状態、または表向きのクライマックスは時計回りに90度（-90）回転
            if (_card.IsRested || (isFaceUp && isClimax))
            {
                zRotation = -90f;
            }

            // Keep RectTransform layout from zone UI. Only update rotation here.
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localRotation = Quaternion.Euler(0, 0, zRotation);
            }
        }
        
        /// <summary>
        /// 設定されているカードのサイド情報に基づいて、適切なカード裏面スプライトを返します。
        /// </summary>
        /// <returns>カード裏面のスプライト。</returns>
        private Sprite GetCardBackSprite()
        {
            if (_card != null && _card.Data != null)
            {
                string side = _card.Data.Side;
                if (side == "Weiss" || side == "ヴァイス") return _weissCardBackSprite;
                if (side == "Schwarz" || side == "シュヴァルツ") return _schwarzCardBackSprite;
            }
            return _weissCardBackSprite; // デフォルトとしてヴァイスの裏面を返す
        }
    }
}
