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
    public class CardDetailView : MonoBehaviour
    {
        [SerializeField] private Image cardImageView;
        [SerializeField] private TextMeshProUGUI cardDetailText;
        [SerializeField] private Button closeButton;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            closeButton?.onClick.AddListener(Hide);
            Hide(); // Start hidden
        }

        /// <summary>
        /// Displays the detailed information for the given WeissCard.
        /// </summary>
        /// <param name="card">The WeissCard to display.</param>
        public void Show(WeissCard card)
        {
            if (card == null)
            {
                Debug.LogError("CardDetailView.Show: Card is null.");
                return;
            }

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
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            Debug.Log("CardDetailView: Hidden.");
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
                        sb.AppendLine($"  [Generic Ability] {abilityBase.SourceCard.Data.name}");
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
    }
}
