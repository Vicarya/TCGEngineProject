using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Waiting Room (Discard Pile).
    /// </summary>
    public class WaitingRoomUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 20f;
        [SerializeField] private int maxCardsToShow = 10; // To prevent performance issues

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// Updates the Waiting Room display.
        /// </summary>
        /// <param name="waitingRoomZone">The waiting room zone from the game logic.</param>
        public void UpdateZone(WaitingRoomZone waitingRoomZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (waitingRoomZone == null) return;

            // Show only the top N cards to avoid clutter and performance issues
            var cardsToShow = waitingRoomZone.Cards.Take(maxCardsToShow);
            int i = 0;
            foreach(var card in cardsToShow)
            {
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // Waiting room cards are always face up
                    card.IsFaceUp = true;
                    cardUI.SetCard(card);
                    _cardUIs.Add(cardUI);

                    // Apply basic horizontal layout
                    var rectTransform = cardObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(i * cardSpacing, 0);
                    }
                }
                else
                {
                    Debug.LogError("Card prefab is missing CardUI component!");
                    Destroy(cardObject);
                }
                i++;
            }
        }
    }
}
