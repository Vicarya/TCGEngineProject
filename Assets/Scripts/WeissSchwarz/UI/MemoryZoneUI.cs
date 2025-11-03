using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Memory Zone.
    /// </summary>
    public class MemoryZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 30f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// Updates the Memory Zone display.
        /// </summary>
        /// <param name="memoryZone">The memory zone from the game logic.</param>
        public void UpdateZone(MemoryZone memoryZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (memoryZone == null) return;

            for (int i = 0; i < memoryZone.MemoryCards.Count; i++)
            {
                var memoryCardInfo = memoryZone.MemoryCards[i];
                var card = memoryCardInfo.Card;

                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // Set the face-up state before setting the card data
                    card.IsFaceUp = memoryCardInfo.FaceUp;
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
