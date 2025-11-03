using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Stock Zone.
    /// </summary>
    public class StockZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 cardOffset = new Vector2(10, -5); // How much each card in the stack is offset

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// Updates the Stock Zone display.
        /// </summary>
        /// <param name="stockZone">The stock zone from the game logic.</param>
        public void UpdateZone(StockZone stockZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (stockZone == null) return;

            // Stock is LIFO, but for UI we can just show the count. We'll show them in order for visual flair.
            for (int i = 0; i < stockZone.Cards.Count; i++)
            {
                var card = stockZone.Cards[i];
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // Stock cards are always face down
                    card.IsFaceUp = false;
                    cardUI.SetCard(card);
                    _cardUIs.Add(cardUI);

                    // Apply stacking layout
                    var rectTransform = cardObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = i * cardOffset;
                    }
                }
                else
                {
                    Debug.LogError("Card prefab is missing CardUI component!");
                    Destroy(cardObject);
                }
            }
        }
    }
}
