using System.Linq;
using TCG.Weiss;
using TMPro;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 山札置場のUI表示を管理するMonoBehaviourクラス。
    /// </summary>
    public class DeckZoneUI : MonoBehaviour
    {
        [Header("UI参照")]
        /// <summary>
        /// 山札のカードの山（一番上のカード）を表すCardUI。
        /// </summary>
        [SerializeField] private CardUI deckCardUI; // Represents the pile of cards
        /// <summary>
        /// 山札の枚数を表示するTextMeshProUGUI。
        /// </summary>
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
