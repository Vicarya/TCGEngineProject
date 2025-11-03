using System.Linq;
using TCG.Weiss;
using TMPro;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Deck Zone.
    /// </summary>
    public class DeckZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CardUI deckCardUI; // Represents the pile of cards
        [SerializeField] private TextMeshProUGUI deckCountText;

        /// <summary>
        /// Updates the Deck Zone display.
        /// </summary>
        /// <param name="deckZone">The deck zone from the game logic.</param>
        public void UpdateZone(DeckZone deckZone)
        {
            if (deckZone == null)
            {
                if(deckCardUI != null) deckCardUI.gameObject.SetActive(false);
                if(deckCountText != null) deckCountText.text = "0";
                return;
            }

            if (deckCountText != null)
            {
                deckCountText.text = deckZone.Cards.Count.ToString();
            }

            if (deckCardUI != null)
            {
                bool hasCards = deckZone.Cards.Any();
                deckCardUI.gameObject.SetActive(hasCards);

                if (hasCards)
                {
                    // Show a representation of the deck (the top card, face down)
                    var topCard = deckZone.Cards.First();
                    topCard.IsFaceUp = false;
                    deckCardUI.SetCard(topCard);
                }
            }
        }
    }
}
