using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Weiss;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 構築されたデッキリスト内で単一のカード項目を表示するUIコンポーネント。
    /// カード名、枚数を表示し、カードを1枚削除するボタンを提供します。
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
        /// カードが削除されたときに呼び出されるアクション。
        /// </summary>
        private Action<WeissCardData> _removeAction;

        /// <summary>
        /// オブジェクト初期化時に呼び出され、削除ボタンにリスナーを追加します。
        /// </summary>
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnRemoveButtonClicked);
        }

        /// <summary>
        /// このデッキリスト項目の表示とアクションを設定します。
        /// </summary>
        /// <param name="cardData">表示するカードデータ。</param>
        /// <param name="count">デッキ内のカード枚数。</param>
        /// <param name="removeAction">削除ボタンがクリックされたときに呼び出すアクション。</param>
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

        /// <summary>
        /// 削除ボタンがクリックされたときに呼び出されます。
        /// 設定された削除アクションを実行します。
        /// </summary>
        private void OnRemoveButtonClicked()
        {
            _removeAction?.Invoke(_cardData);
        }
    }
}
