using UnityEngine;
using TMPro; // For TextMeshProUGUI
using UnityEngine.UI; // For Button

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Represents a single card item in the Deck Editor's scrollable list.
    /// </summary>
    public class CardListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardNoText;
        [SerializeField] private Button selectButton;

        private WeissCardData _cardData;

        private void Awake()
        {
            selectButton?.onClick.AddListener(OnCardSelected);
        }

        public void SetCardData(WeissCardData data)
        {
            _cardData = data;
            if (cardNameText != null) cardNameText.text = data.name;
            if (cardNoText != null) cardNoText.text = data.card_no;
        }

        private void OnCardSelected()
        {
            if (_cardData != null)
            {
                // When a card in the master list is selected, add it to the deck.
                DeckEditorManager.Instance?.AddCardToDeck(_cardData);
            }
        }
    }
}
