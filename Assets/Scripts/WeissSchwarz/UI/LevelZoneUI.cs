using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the UI representation of the Level Zone.
    /// </summary>
    public class LevelZoneUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab; // Should have a CardUI component
        [SerializeField] private Transform cardContainer;

        [Header("Layout Settings")]
        [SerializeField] private float cardSpacing = 100f;

        private List<CardUI> _cardUIs = new List<CardUI>();

        /// <summary>
        /// Updates the Level Zone display.
        /// </summary>
        /// <param name="levelZone">The level zone from the game logic.</param>
        public void UpdateZone(LevelZone levelZone)
        {
            // Clear existing UI objects
            foreach (var cardUI in _cardUIs)
            {
                Destroy(cardUI.gameObject);
            }
            _cardUIs.Clear();

            if (levelZone == null) return;

            for (int i = 0; i < levelZone.LevelCards.Count; i++)
            {
                var levelCardInfo = levelZone.LevelCards[i];
                var card = levelCardInfo.Card;

                var cardObject = Instantiate(cardPrefab, cardContainer);
                var cardUI = cardObject.GetComponent<CardUI>();

                if (cardUI != null)
                {
                    // IMPORTANT: Set the face-up state before setting the card data
                    card.IsFaceUp = levelCardInfo.FaceUp;
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
