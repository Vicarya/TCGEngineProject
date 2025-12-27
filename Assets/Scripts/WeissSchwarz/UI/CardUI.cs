using TCG.Core;
using TCG.Weiss;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲームフィールド上に表示される単一のヴァイスシュヴァルツカードの視覚的な表現を管理するMonoBehaviourクラス。
    /// カードの表/裏、レスト状態、クリックへの応答などを制御します。
    /// 表と裏のGameObjectを分けることで、アニメーションなどを実装しやすい階層構造を採用しています。
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [Header("カードデータ")]
        private WeissCard _card; // このUIが表現するWeissCardインスタンスへの参照

        // カードの裏面やデフォルト画像のスプライト
        private Sprite _weissCardBackSprite;
        private Sprite _schwarzCardBackSprite;
        private Sprite _defaultCardSprite;

        [Header("UI子オブジェクト")]
        [SerializeField] private GameObject cardFrontObject; // カード表面のImageコンポーネントを持つGameObject
        [SerializeField] private GameObject cardBackObject;  // カード裏面のImageコンポーネントを持つGameObject
        [SerializeField] private Button cardButton; // カードクリックを検出するためのButtonコンポーネント

        // ImageコンポーネントはAwake()で子オブジェクトから取得
        private Image _cardFrontImage;
        private Image _cardBackImage;

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
            if (_card != null)
            {
                GameView.Instance.ShowCardDetail(_card);
            }
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
                if(_cardBackImage != null) _cardBackImage.sprite = GetCardBackSprite();
            }
            
            // レスト状態に応じてカードの回転を更新
            // カードがレスト状態であれば90度回転、そうでなければ元の角度に戻す
            transform.localRotation = _card.IsRested ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
        }

        /// <summary>
        /// 設定されているカードのサイド情報に基づいて、適切なカード裏面スプライトを返します。
        /// </summary>
        /// <returns>カード裏面のスプライト。</returns>
        private Sprite GetCardBackSprite()
        {
            if (_card != null && _card.Data.Metadata.TryGetValue("サイド", out object sideObject))
            { 
                if (sideObject is string sideString)
                {
                    if (sideString == "ヴァイス") return _weissCardBackSprite;
                    if (sideString == "シュヴァルツ") return _schwarzCardBackSprite;
                }
            }
            return _weissCardBackSprite; // デフォルトとしてヴァイスの裏面を返す
        }
    }
}
