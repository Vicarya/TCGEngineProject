using TCG.Weiss;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Represents the UI for a single card on the game board.
    /// It holds references to the UI elements that display card information.
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [Header("Card Info")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI soulText;
        [SerializeField] private Image cardImage; // For card art

        [Header("State Visuals")]
        [SerializeField] private GameObject restedOverlay; // e.g., a semi-transparent panel or rotation

        private WeissCard _weissCard;

        /// <summary>
        /// Updates the UI elements with the data from the provided card.
        /// </summary>
        /// <param name="card">The card data to display.</param>
        public void Initialize(WeissCard card)
        {
            _weissCard = card;
            var cardData = card.Data as WeissCardData;

            if (cardData == null)
            {
                // Clear display if card data is invalid
                cardNameText.text = "Unknown";
                powerText.text = "";
                soulText.text = "";
                // if (cardImage != null) cardImage.sprite = null; // TODO: Set to a default card back
                return;
            }

            // Update UI elements
            if (cardNameText != null) cardNameText.text = cardData.Name;
            if (powerText != null) powerText.text = cardData.Power.ToString();
            if (soulText != null) soulText.text = cardData.Soul.ToString();
            // if (cardImage != null) cardImage.sprite = ... // TODO: Load sprite from resources based on card ID

            // Update visual state
            UpdateVisualState();
        }

        /// <summary>
        /// Updates the visual state of the card (e.g., rested/stand).
        /// </summary>
        public void UpdateVisualState()
        {
            if (_weissCard == null) return;

            bool isRested = _weissCard.IsTapped;
            if (restedOverlay != null)
            {
                restedOverlay.SetActive(isRested);
            }
            else
            {
                // Alternatively, rotate the card to show it's rested
                transform.localRotation = isRested ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
            }
        }
    }
}
