using TCG.Core;
using TCG.Weiss;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲームフィールド上に表示される単一のヴァイスシュヴァルツカードの視覚的な表現を管理するMonoBehaviourクラス。
    /// カードの表/裏、レスト状態、クリックへの応答などを制御します。
    /// 表と裏のGameObjectを分けることで、アニメーションなどを実装しやすい階層構造を採用しています。
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("カードデータ")]
        private WeissCard _card; // このUIが表現するWeissCardインスタンスへの参照
        public WeissCard Card => _card;

        // カードの裏面やデフォルト画像のスプライト
        private Sprite _weissCardBackSprite;
        private Sprite _schwarzCardBackSprite;
        private Sprite _defaultCardSprite;

        [Header("UI子オブジェクト")]
        [SerializeField] private GameObject cardFrontObject; // カード表面のImageコンポーネントを持つGameObject
        [SerializeField] private GameObject cardBackObject;  // カード裏面のImageコンポーネントを持つGameObject
        [SerializeField] private Button cardButton; // カードクリックを検出するためのButtonコンポーネント
        [SerializeField] private GameObject selectedOverlayObject; // 選択中ハイライト表示

        // ImageコンポーネントはAwake()で子オブジェクトから取得
        private Image _cardFrontImage;
        private Image _cardBackImage;
        private bool _isSelectionMode;
        private bool _isSelected;
        private readonly Color _normalTint = Color.white;
        private readonly Color _selectedTint = new Color(1f, 0.55f, 0.55f, 1f);

        public bool IsSelected => _isSelected;
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
            cardButton?.onClick.AddListener(OnCardClicked);
            SetSelected(false);

            // 必要なコンポーネントやリソースが見つからない場合の警告
            if (_cardFrontImage == null) Debug.LogError("カード表面のGameObjectにImageコンポーネントが見つかりません。", this);
            if (_cardBackImage == null) Debug.LogError("カード裏面のGameObjectにImageコンポーネントが見つかりません。", this);
            if (_weissCardBackSprite == null) Debug.LogWarning($"ヴァイスのカード裏面スプライトが'Resources/{weissCardBackPath}'に見つかりません。");
            if (_schwarzCardBackSprite == null) Debug.LogWarning($"シュヴァルツのカード裏面スプライトが'Resources/{schwarzCardBackPath}'に見つかりません。");
            if (_defaultCardSprite == null) Debug.LogWarning($"デフォルトカードスプライトが'Resources/{defaultCardImagePath}'に見つかりません。");
        }

        /// <summary>
        /// このCardUIが表現するWeissCardインスタンスを設定し、見た目を更新します。
        /// </summary>
        /// <param name="card">設定するWeissCardインスタンス。</param>
        public void SetCard(WeissCard card)
        {
            _card = card;
            UpdateCardVisuals(); // カードデータに基づいて見た目を更新
        }

        /// <summary>
        /// カードがクリックされたときに呼び出されます。
        /// クリックされたカードの詳細をGameView経由で表示します。
        /// </summary>
        private void OnCardClicked()
        {
            CardClicked?.Invoke(this);

            if (_isSelectionMode)
            {
                ToggleSelected();
                return;
            }

            if (_card != null)
            {
                GameView.Instance.ShowCardDetail(_card);
            }
        }

        /// <summary>
        /// Button未割り当てでもクリックを拾えるようにするフォールバック。
        /// </summary>
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
        }

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

        public void ToggleSelected()
        {
            SetSelected(!_isSelected);
        }

        /// <summary>
        /// 現在設定されているWeissCardの状態に基づいて、カードの視覚的な表示（表/裏、画像、回転など）を更新します。
        /// </summary>
        private void UpdateCardVisuals()
        {
            if (_card == null)
            {
                gameObject.SetActive(false); // カードがない場合は非表示にする
                return;
            }

            gameObject.SetActive(true); // カードがある場合は表示する
            bool isFaceUp = _card.IsFaceUp; // カードが表向きかどうか

            // カードの表裏表示を切り替える
            if (cardFrontObject != null) cardFrontObject.SetActive(isFaceUp);
            if (cardBackObject != null) cardBackObject.SetActive(!isFaceUp);

            if (isFaceUp)
            {
                // 表向きの場合、カードの表面画像をロードして表示
                var cardData = _card.Data;
                Sprite spriteToDisplay = null;

                // アスペクト比を維持
                if (_cardFrontImage != null) _cardFrontImage.preserveAspect = true;

                if (!string.IsNullOrEmpty(cardData.ImagePath))
                {
                    spriteToDisplay = Resources.Load<Sprite>(cardData.ImagePath);
                }

                if (spriteToDisplay == null)
                {
                    spriteToDisplay = _defaultCardSprite; // 画像が見つからない場合はデフォルト画像
                }

                if(_cardFrontImage != null) _cardFrontImage.sprite = spriteToDisplay;
            }
            else
            {
                // 裏向きの場合、カードの裏面画像を表示（サイドによって切り替え）
                if (_cardBackImage != null)
                {
                    _cardBackImage.preserveAspect = true;
                    _cardBackImage.sprite = GetCardBackSprite();
                }
            }

            // 回転状態の更新
            float zRotation = 0f;
            bool isClimax = _card.Data != null && (_card.Data.CardType == "Climax" || _card.Data.CardType == "クライマックス");

            if (_card.IsRested || (isFaceUp && isClimax))
            {
                // レスト状態、または表向きのクライマックスは時計回りに90度（-90）回転
                zRotation = -90f;
            }

            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                // 位置ズレ防止のためアンカーを中央にリセット
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                
                rect.localRotation = Quaternion.Euler(0, 0, zRotation);

                // 補正係数を 1.4f (88/63) に調整。これでキャラクターカードと同じ高さになります。
                float scaleComp = (isFaceUp && isClimax) ? 1.4f : 1f;
                rect.localScale = new Vector3(scaleComp, scaleComp, 1f);
            }
        }

        /// <summary>
        /// 設定されているカードのサイド情報に基づいて、適切なカード裏面スプライトを返します。
        /// </summary>
        /// <returns>カード裏面のスプライト。</returns>
        private Sprite GetCardBackSprite()
        {
            if (_card != null)
            {
                string side = _card.Data.Side;
                if (side == "Weiss" || side == "ヴァイス") return _weissCardBackSprite;
                if (side == "Schwarz" || side == "シュヴァルツ") return _schwarzCardBackSprite;
            }
            return _weissCardBackSprite; // デフォルトとしてヴァイスの裏面を返す
        }
    }
}
