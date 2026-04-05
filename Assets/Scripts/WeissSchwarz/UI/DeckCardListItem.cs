using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 構築されたデッキリスト内で単一のカード項目を表示するUIコンポーネント。
    /// カード名、枚数を表示し、クリックするとカード詳細を表示します。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DeckCardListItem : MonoBehaviour
    {
        /// <summary>
        /// カード名を表示するTextMeshProUGUI。
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardNameText;
        /// <summary>
        /// デッキ内のカード枚数を表示するTextMeshProUGUI。
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardCountText;

        /// <summary>
        /// このUI項目に関連付けられたカードデータ。
        /// </summary>
        private WeissCardData _cardData;

        /// <summary>
        /// オブジェクト初期化時に呼び出され、クリックイベントにリスナーを追加します。
        /// </summary>
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        /// <summary>
        /// このデッキリスト項目の表示を設定します。
        /// </summary>
        /// <param name="cardData">表示するカードデータ。</param>
        /// <param name="count">デッキ内のカード枚数。</param>
        public void Setup(WeissCardData cardData, int count)
        {
            _cardData = cardData;

            if (cardNameText != null)
            {
                cardNameText.text = _cardData.Name;
            }

            if (cardCountText != null)
            {
                cardCountText.text = $"x{count}";
            }

            var image = GetComponent<Image>();
            if (image != null && _cardData != null)
            {
                // アスペクト比維持と回転の設定
                image.preserveAspect = true;
                image.sprite = null;

                RectTransform rect = image.rectTransform;
                if (rect != null)
                {
                    bool isClimax = _cardData.CardType == "Climax" || _cardData.CardType == "クライマックス";
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.localRotation = isClimax ? Quaternion.Euler(0, 0, -90f) : Quaternion.identity;

                    // クライマックスカードのスケールを 1.4f に統一
                    float scale = isClimax ? 1.4f : 1f;
                    rect.localScale = new Vector3(scale, scale, 1f);
                }

                StartCoroutine(ImageLoader.LoadImage(_cardData.ImagePath, image));
            }
        }

        /// <summary>
        /// リスト項目がクリックされたときに呼び出されます。
        /// カード詳細ビューを表示します。
        /// </summary>
        private void OnClick()
        {
            if (_cardData != null)
            {
                DeckEditorManager.Instance.ShowCardDetail(_cardData);
            }
        }
    }
}
