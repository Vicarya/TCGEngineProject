using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 構築されたデッキリスト内で単一のカード項目を表示するUIコンポーネント。
    /// カード名、枚数を表示し、クリックするとカード詳細を表示します。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DeckCardListItem : MonoBehaviour
    {
        /// <summary>
        /// カード名を表示するTextMeshProUGUI。
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardNameText;
        /// <summary>
        /// デッキ内のカード枚数を表示するTextMeshProUGUI。
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardCountText;

        /// <summary>
        /// このUI項目に関連付けられたカードデータ。
        /// </summary>
        private WeissCardData _cardData;

        /// <summary>
        /// オブジェクト初期化時に呼び出され、クリックイベントにリスナーを追加します。
        /// </summary>
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        /// <summary>
        /// このデッキリスト項目の表示を設定します。
        /// </summary>
        /// <param name="cardData">表示するカードデータ。</param>
        /// <param name="count">デッキ内のカード枚数。</param>
        public void Setup(WeissCardData cardData, int count)
        {
            _cardData = cardData;

            if (cardNameText != null)
            {
                cardNameText.text = _cardData.name;
            }

            if (cardCountText != null)
            {
                cardCountText.text = $"x{count}";
            }
        }

        /// <summary>
        /// リスト項目がクリックされたときに呼び出されます。
        /// カード詳細ビューを表示します。
        /// </summary>
        private void OnClick()
        {
            if (_cardData != null)
            {
                DeckEditorManager.Instance.ShowCardDetail(_cardData);
            }
        }
    }
}
