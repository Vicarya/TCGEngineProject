using TCG.Weiss;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// デッキエディタのグリッドにカード画像を表示するUIです。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardGridItem : MonoBehaviour
    {
        private Image _cardImage;
        private Button _button;
        private WeissCardData _cardData;

        private void Awake()
        {
            _cardImage = gameObject.GetComponent<Image>();
            if (_cardImage == null)
            {
                _cardImage = gameObject.AddComponent<Image>();
            }

            _button = gameObject.GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
            }
        }

        public void Initialize(WeissCardData cardData)
        {
            _cardData = cardData;
            gameObject.name = _cardData.CardCode;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnCardClick);

            if (_cardImage != null)
            {
                _cardImage.preserveAspect = true;
                _cardImage.sprite = null;

                RectTransform rect = _cardImage.rectTransform;
                if (rect != null)
                {
                    CardUI.ApplyCardImagePresentation(rect, CardUI.IsClimaxCard(_cardData));
                }
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ImageLoader.LoadImage(_cardData.ImagePath, _cardImage));
            }
        }

        private void OnCardClick()
        {
            if (_cardData != null)
            {
                DeckEditorManager.Instance.ShowCardDetail(_cardData);
            }
        }
    }
}
