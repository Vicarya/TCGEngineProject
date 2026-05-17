using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// デッキ一覧で1枚のカード情報を表示するUIです。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DeckCardListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardCountText;

        private WeissCardData _cardData;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

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
            if (image == null || _cardData == null)
            {
                return;
            }

            image.preserveAspect = true;
            image.sprite = null;

            RectTransform rect = image.rectTransform;
            if (rect != null)
            {
                CardUI.ApplyCardImagePresentation(rect, CardUI.IsClimaxCard(_cardData));
            }

            StartCoroutine(ImageLoader.LoadImage(_cardData.ImagePath, image));
        }

        private void OnClick()
        {
            if (_cardData != null)
            {
                DeckEditorManager.Instance.ShowCardDetail(_cardData);
            }
        }
    }
}
