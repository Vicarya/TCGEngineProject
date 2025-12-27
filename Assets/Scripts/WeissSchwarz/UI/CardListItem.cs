using UnityEngine;
using TMPro; // TextMeshProUGUIを使用するために必要
using TCG.Weiss;
using UnityEngine.UI; // Buttonを使用するために必要

namespace TCG.Weiss.UI
{
    /// <summary>
    /// デッキエディタのスクロール可能なリスト内で、単一のカード項目（アイテム）を表示・管理するUIコンポーネント。
    /// カード名、カード番号、そして選択ボタンを含みます。
    /// </summary>
    public class CardListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText; // カード名を表示するTextMeshProUGUI
        [SerializeField] private TextMeshProUGUI cardNoText; // カード番号を表示するTextMeshProUGUI
        [SerializeField] private Button selectButton; // このカード項目を選択するためのボタン

        // このUI項目が表示しているカードのデータ
        private WeissCardData _cardData;

        private void Awake()
        {
            // 選択ボタンにクリックリスナーを追加
            selectButton?.onClick.AddListener(OnCardSelected);
        }

        /// <summary>
        /// このリスト項目に表示するカードデータを設定し、UIを更新します。
        /// </summary>
        /// <param name="data">表示するWeissCardData。</param>
        public void SetCardData(WeissCardData data)
        {
            _cardData = data;
            if (cardNameText != null) cardNameText.text = data.name;
            if (cardNoText != null) cardNoText.text = data.card_no;
        }

        /// <summary>
        /// カード項目が選択（ボタンがクリック）されたときに呼び出されます。
        /// 選択されたカードの詳細をDeckEditorManager経由で表示します。
        /// </summary>
        private void OnCardSelected()
        {
            if (_cardData != null)
            {
                // マスターリストのカードが選択された際、その詳細を表示する
                DeckEditorManager.Instance?.ShowCardDetail(_cardData);
            }
        }
    }
}
