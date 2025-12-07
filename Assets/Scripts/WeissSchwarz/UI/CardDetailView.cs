using System.Collections;
using System.Collections.Generic;
using System.Text;
using TCG.Core;
using TCG.Weiss.Effects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro; // For TextMeshProUGUI

namespace TCG.Weiss.UI
{
    /// <summary>
    /// Manages the display of a single card's detailed information.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CardDetailView : MonoBehaviour
    {
        [SerializeField] private Image cardImageView;
        [SerializeField] private TextMeshProUGUI cardDetailText;
        [Tooltip("Text to display \"In Deck: X / 4\"")]
        [SerializeField] private TextMeshProUGUI cardCountText; // Text to display "In Deck: X / 4"
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform buttonContainer; // コンテナを指定
        [SerializeField] private GameObject buttonPrefab; // Prefabを指定

        private List<Button> _createdButtons = new List<Button>();
        private CanvasGroup _canvasGroup;
        private WeissCard _currentCard;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            closeButton?.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Displays the detailed information for the given WeissCard (in-game version).
        /// </summary>
        /// <param name="card">The WeissCard to display.</param>
        public void Show(WeissCard card)
        {
            ClearButtons();

            if (card == null)
            {
                Debug.LogError("CardDetailView.Show: Card is null.");
                return;
            }
            _currentCard = card;

            // Load card image
            if (!string.IsNullOrEmpty(card.Data.image_url))
            {
                StartCoroutine(LoadImage(card.Data.image_url));
            }

            // Format and display card details
            cardDetailText.text = FormatCardDetails(card);

            // Hide deck editor specific UI
            SetDeckEditorUIActive(false);
        }

        /// <summary>
        /// Displays the detailed information for the given WeissCard.
        /// </summary>
        /// <param name="card">The WeissCard to display.</param>
        /// <param name="countInDeck">Number of this card currently in the deck.</param>
        public void Show(WeissCard card, int countInDeck)
        {
            ClearButtons();

            if (card == null)
            {
                Debug.LogError("CardDetailView.Show: Card is null.");
                return;
            }
            _currentCard = card;

            // Load card image
            if (!string.IsNullOrEmpty(card.Data.image_url))
            {
                StartCoroutine(LoadImage(card.Data.image_url));
            }
            else
            {
                cardImageView.sprite = null; // Clear previous image
            }

            // Format and display card details
            cardDetailText.text = FormatCardDetails(card);

            SetDeckEditorUIActive(true);

            UpdateCardCount(countInDeck);

            // Create Add and Remove buttons
            CreateButton("+", () => DeckEditorManager.Instance?.AddCardToDeck(_currentCard.Data));
            CreateButton("-", () => DeckEditorManager.Instance?.RemoveCardFromDeck(_currentCard.Data));

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Debug.Log($"CardDetailView: Displaying details for {card.Data.name}");
        }

        /// <summary>
        /// Hides the card detail view.
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines(); // Stop image loading if it's in progress
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _currentCard = null;
            Debug.Log("CardDetailView: Hidden.");
            ClearButtons();
        }

        /// <summary>
        /// Updates the displayed count of the current card in the deck.
        /// </summary>
        public void UpdateCardCount()
        {
            if (_currentCard != null && gameObject.activeInHierarchy)
            {
                int count = DeckEditorManager.Instance.GetCardCountInDeck(_currentCard.Data);
                UpdateCardCount(count);
            }
        }

        /// <summary>
        /// Updates the displayed count of a specific card if it's the one being shown.
        /// </summary>
        /// <param name="cardData">The card data that was updated.</param>
        public void UpdateCardCount(WeissCardData cardData)
        {
            if (_currentCard != null && _currentCard.Data.card_no == cardData.card_no)
            {
                int count = DeckEditorManager.Instance.GetCardCountInDeck(cardData);
                UpdateCardCount(count);
            }
        }

        private IEnumerator LoadImage(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                cardImageView.sprite = sprite;
            }
            else
            {
                Debug.LogError($"Failed to load image from {url}: {request.error}");
            }
        }

        private string FormatCardDetails(WeissCard card)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<b><size=120%>{card.Data.name}</size></b> ({card.Data.card_no})");
            sb.AppendLine($"<color=#808080>種類: {card.Data.種類}</color>");
            sb.AppendLine($"<color=#808080>レベル: {card.Data.レベル} / コスト: {card.Data.コスト}</color>");
            sb.AppendLine($"<color=#808080>パワー: {card.Data.パワー} / ソウル: {card.Data.ソウル}</color>");
            sb.AppendLine($"<color=#808080>色: {card.Data.色} / サイド: {card.Data.サイド}</color>");
            if (card.Data.特徴 != null && card.Data.特徴.Count > 0)
            {
                sb.AppendLine($"<color=#808080>特徴: {string.Join("・", card.Data.特徴)}</color>");
            }
            sb.AppendLine($"<color=#808080>レアリティ: {card.Data.レアリティ}</color>");
            sb.AppendLine($"<color=#808080>トリガー: {card.Data.トリガー}</color>");
            sb.AppendLine();

            if (card.Abilities != null && card.Abilities.Count > 0)
            {
                sb.AppendLine("<b>--- Abilities ---</b>");
                foreach (var abilityBase in card.Abilities)
                {
                    if (abilityBase is WeissAbility ability)
                    {
                        sb.AppendLine($"<b>[{ability.AbilityType}]</b>");
                        if (ability.Costs != null && ability.Costs.Count > 0)
                        {
                            sb.Append("  Cost: ");
                            foreach (var cost in ability.Costs)
                            {
                                sb.Append($"[{cost.GetDescription()}] ");
                            }
                            sb.AppendLine();
                        }
                        sb.AppendLine($"  Effect: {ability.Description}");
                        // For now, we just print the raw description.
                        // In the future, we can iterate through ability.Effects and describe them.
                    }
                    else
                    {
                        // SourceCard may be a CardBase<WeissCardData> or other concrete type. Cast safely.
                        if (abilityBase.SourceCard is TCG.Core.CardBase<TCG.Weiss.WeissCardData> srcWithData)
                        {
                            sb.AppendLine($"  [Generic Ability] {srcWithData.Data.name}");
                        }
                        else
                        {
                            sb.AppendLine($"  [Generic Ability] (unknown source)");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(card.Data.flavor_text))
            {
                sb.AppendLine();
                sb.AppendLine($"<i>{card.Data.flavor_text}</i>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a button with the given text and action.
        /// </summary>
        /// <param name="text">Text to display on the button.</param>
        /// <param name="action">Action to perform when the button is clicked.</param>
        private void CreateButton(string text, UnityEngine.Events.UnityAction action)
        {
            if (buttonPrefab == null || buttonContainer == null)
            {
                Debug.LogError("Button prefab or button container is not assigned.");
                return;
            }

            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (button != null && buttonText != null)
            {
                buttonText.text = text;
                button.onClick.AddListener(action);
                _createdButtons.Add(button);
            }
            else
            {
                Debug.LogError("Failed to initialize button from prefab.");
            }
        }
        private void SetDeckEditorUIActive(bool isActive)
        {
            if (cardCountText != null)
                cardCountText.gameObject.SetActive(isActive);

            if (buttonContainer != null)
                buttonContainer.gameObject.SetActive(isActive);
        }

        private void ClearButtons()
        {
            foreach (var button in _createdButtons)
            {
                Destroy(button.gameObject);
            }
            _createdButtons.Clear();
        }
        private void UpdateCardCount(int count)
        {
            if (cardCountText != null)
            {
                cardCountText.text = $"デッキ内: {count} / 4";
            }
        }
    }
}
