using TCG.Core;
using TCG.Weiss;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the visual representation of a single Weiss Schwarz card.
    /// This version uses a sibling structure for its GameObjects to be animation-friendly.
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [Header("Card Data")]
        private WeissCard _card;
        private Sprite _weissCardBackSprite;
        private Sprite _schwarzCardBackSprite;
        private Sprite _defaultCardSprite;

        [Header("UI Child GameObjects")]
        [SerializeField] private GameObject cardFrontObject; // GameObject containing the front Image
        [SerializeField] private GameObject cardBackObject;  // GameObject containing the back Image
        [SerializeField] private Button cardButton; // Button component for click detection

        // Image components are fetched from the GameObjects
        private Image _cardFrontImage;
        private Image _cardBackImage;

        [Header("Resource Paths")]
        [SerializeField] private string weissCardBackPath = "Images/WeissCardBack";
        [SerializeField] private string schwarzCardBackPath = "Images/SchwarzCardBack";
        [SerializeField] private string defaultCardImagePath = "Images/DefaultCard";

        private void Awake()
        {
            // Load sprites
            _weissCardBackSprite = Resources.Load<Sprite>(weissCardBackPath);
            _schwarzCardBackSprite = Resources.Load<Sprite>(schwarzCardBackPath);
            _defaultCardSprite = Resources.Load<Sprite>(defaultCardImagePath);

            // Get Image components from child objects
            if (cardFrontObject != null) _cardFrontImage = cardFrontObject.GetComponent<Image>();
            if (cardBackObject != null) _cardBackImage = cardBackObject.GetComponent<Image>();

            // Add click listener
            cardButton?.onClick.AddListener(OnCardClicked);

            // Warnings
            if (_cardFrontImage == null) Debug.LogError("Card Front Object is missing an Image component.", this);
            if (_cardBackImage == null) Debug.LogError("Card Back Object is missing an Image component.", this);
            if (_weissCardBackSprite == null) Debug.LogWarning($"Weiss card back sprite not found at 'Resources/{weissCardBackPath}'.");
            if (_schwarzCardBackSprite == null) Debug.LogWarning($"Schwarz card back sprite not found at 'Resources/{schwarzCardBackPath}'.");
            if (_defaultCardSprite == null) Debug.LogWarning($"Default card sprite not found at 'Resources/{defaultCardImagePath}'.");
        }

        public void SetCard(WeissCard card)
        {
            _card = card;
            UpdateCardVisuals();
        }

        private void OnCardClicked()
        {
            if (_card != null)
            {
                GameView.Instance.ShowCardDetail(_card);
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

            // Enable/disable the correct child GameObject
            if (cardFrontObject != null) cardFrontObject.SetActive(isFaceUp);
            if (cardBackObject != null) cardBackObject.SetActive(!isFaceUp);

            if (isFaceUp)
            {
                // Set the front image sprite
                var cardData = _card.Data;
                Sprite spriteToDisplay = null;

                if (!string.IsNullOrEmpty(cardData.ImagePath))
                {
                    spriteToDisplay = Resources.Load<Sprite>(cardData.ImagePath);
                }

                if (spriteToDisplay == null)
                {
                    spriteToDisplay = _defaultCardSprite;
                }

                if(_cardFrontImage != null) _cardFrontImage.sprite = spriteToDisplay;
            }
            else
            {
                // Set the back image sprite
                if(_cardBackImage != null) _cardBackImage.sprite = GetCardBackSprite();
            }
            
            // Update rested state by rotating the root object
            transform.localRotation = _card.IsRested ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
        }

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
            return _weissCardBackSprite; // Default
        }
    }
}
