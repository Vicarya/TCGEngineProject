using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Weiss;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Represents a single card item in the constructed deck list.
    /// Displays card name, count, and provides a button to remove one copy.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DeckCardListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardCountText;

        private WeissCardData _cardData;
        private Action<WeissCardData> _removeAction;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnRemoveButtonClicked);
        }

        /// <summary>
        /// Sets up the display and action for this deck list item.
        /// </summary>
        /// <param name="cardData">The card data to display.</param>
        /// <param name="count">The number of copies in the deck.</param>
        /// <param name="removeAction">The action to call when the remove button is clicked.</param>
        public void Setup(WeissCardData cardData, int count, Action<WeissCardData> removeAction)
        {
            _cardData = cardData;
            _removeAction = removeAction;

            if (cardNameText != null)
            {
                cardNameText.text = _cardData.name;
            }

            if (cardCountText != null)
            {
                cardCountText.text = $"x{count}";
            }
        }

        private void OnRemoveButtonClicked()
        {
            _removeAction?.Invoke(_cardData);
        }
    }
}
