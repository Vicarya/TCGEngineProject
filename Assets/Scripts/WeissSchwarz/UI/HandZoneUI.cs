using System.Collections.Generic;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of a player's hand.
    /// </summary>
    public class HandZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 100f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        // --- Mulligan Selection ---
        public void EnterMulliganSelectionMode()
        {
            Debug.Log("HandZoneUI: Entering mulligan selection mode.");
            // Implementation to follow: add click listeners to each CardUI
        }

        public List<WeissCard> GetSelectedCardsForMulligan()
        {
            Debug.Log("HandZoneUI: Getting selected cards for mulligan.");
            // Implementation to follow: get selected cards from _cardUIs
            return new List<WeissCard>();
        }
        // --------------------------

        /// <summary>
        /// Updates the hand display to match the provided HandZone state.
        /// </summary>
        /// <param name="handZone">The hand zone from the game logic.</param>
        public void UpdateZone(HandZone handZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            // Create new UI objects for each card in the hand
            if (handZone == null) return;

            for (int i = 0; i < handZone.Cards.Count; i++)
            {
                var card = handZone.Cards[i];
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    cardUI.SetCard(card as WeissCard);
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
            }
        }
    }
}
