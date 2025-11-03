using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Clock Zone.
    /// </summary>
    public class ClockZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 80f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// Updates the Clock Zone display.
        /// </summary>
        /// <param name="clockZone">The clock zone from the game logic.</param>
        public void UpdateZone(ClockZone clockZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (clockZone == null) return;

            for (int i = 0; i < clockZone.Cards.Count; i++)
            {
                var card = clockZone.Cards[i];
                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // Clock cards are always face up
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
            }
        }
    }
}
