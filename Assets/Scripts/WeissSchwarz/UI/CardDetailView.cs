using System.Collections;
using System.Collections.Generic;
using System.Text;
using TCG.Core;
using TCG.Weiss.Effects;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUIを使用するために必要

namespace TCG.Weiss.UI
{
    /// <summary>
    /// 単一のヴァイスシュヴァルツカードの詳細情報を表示するUIビューを管理するMonoBehaviourクラス。
    /// カード画像、詳細テキスト、デッキ内の枚数表示、デッキ編集ボタンなどを制御します。
    /// ゲームプレイ中とデッキエディタ画面の両方での利用を想定しています。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))] // CanvasGroupコンポーネントが必須であることを指定
    public class CardDetailView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image cardImageView; // カード画像を表示するImageコンポーネント
        [SerializeField] private TextMeshProUGUI cardDetailText; // カードの詳細情報を表示するTextMeshProUGUI
        [Tooltip("「デッキ内: X / 4」のテキストを表示")]
        [SerializeField] private TextMeshProUGUI cardCountText; // デッキ内のカード枚数を表示するTextMeshProUGUI
        [SerializeField] private Button closeButton; // 詳細ビューを閉じるボタン
        [SerializeField] private Transform buttonContainer; // デッキ編集ボタンを配置するコンテナ
        [SerializeField] private GameObject buttonPrefab; // 動的に生成するボタンのPrefab

        // 内部状態
        private List<Button> _createdButtons = new List<Button>(); // 生成されたボタンのリスト
        private CanvasGroup _canvasGroup; // UIの表示/非表示、インタラクトを制御するためのCanvasGroup
        private WeissCard _currentCard; // 現在表示中のWeissCardインスタンス

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroupコンポーネントを取得
            closeButton?.onClick.AddListener(Hide); // 閉じるボタンにHideメソッドをリスナーとして追加
        }

        /// <summary>
        /// ゲームプレイ中のカード（WeissCardインスタンス）の詳細情報を表示します。
        /// デッキ編集特有のUIは非表示になります。
        /// </summary>
        /// <param name="card">表示するWeissCardインスタンス。</param>
        public void Show(WeissCard card)
        {
            ClearButtons(); // 既存のボタンをクリア

            if (card == null)
            {
                Debug.LogError("CardDetailView.Show: 表示するカードがnullです。");
                return;
            }
            _currentCard = card; // 現在のカードとして保持

            // カード画像を非同期でロード
            if (!string.IsNullOrEmpty(card.Data.image_url))
            {
                StartCoroutine(LoadImage(card.Data.image_url));
            }
            else
            {
                cardImageView.sprite = null; // 画像URLがない場合は以前の画像をクリア
            }

            // カード詳細を整形して表示
            cardDetailText.text = FormatCardDetails(card);

            // デッキ編集UIを非表示に設定
            SetDeckEditorUIActive(false);

            // CanvasGroupを介してUIを表示状態にする
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Debug.Log($"CardDetailView: {card.Data.name} の詳細を表示中。");
        }

        /// <summary>
        /// デッキエディタ用のカード詳細情報を表示します。
        /// デッキ内の枚数表示や、デッキへの追加/削除ボタンが有効になります。
        /// </summary>
        /// <param name="card">表示するWeissCardインスタンス。</param>
        /// <param name="countInDeck">現在デッキ内にあるこのカードの枚数。</param>
        public void Show(WeissCard card, int countInDeck)
        {
            ClearButtons(); // 既存のボタンをクリア

            if (card == null)
            {
                Debug.LogError("CardDetailView.Show: 表示するカードがnullです。");
                return;
            }
            _currentCard = card; // 現在のカードとして保持

            // カード画像を非同期でロード
            if (!string.IsNullOrEmpty(card.Data.image_url))
            {
                StartCoroutine(LoadImage(card.Data.image_url));
            }
            else
            {
                cardImageView.sprite = null; // 以前の画像をクリア
            }

            // カード詳細を整形して表示
            cardDetailText.text = FormatCardDetails(card);

            // デッキ編集UIを有効に設定
            SetDeckEditorUIActive(true);

            // デッキ内のカード枚数表示を更新
            UpdateCardCount(countInDeck);

            // デッキ追加/削除ボタンを動的に生成
            // DeckEditorManagerのインスタンス経由でデッキ操作メソッドを呼び出す
            CreateButton("+", () => DeckEditorManager.Instance?.AddCardToDeck(_currentCard.Data));
            CreateButton("-", () => DeckEditorManager.Instance?.RemoveCardFromDeck(_currentCard.Data));

            // CanvasGroupを介してUIを表示状態にする
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            Debug.Log($"CardDetailView: {card.Data.name} の詳細を表示中。");
        }

        /// <summary>
        /// カード詳細ビューを非表示にします。
        /// 画像ロード中のコルーチンも停止します。
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines(); // 画像ロード中のコルーチンを停止
            _canvasGroup.alpha = 0f; // 透明にして非表示
            _canvasGroup.interactable = false; // 非インタラクティブに
            _canvasGroup.blocksRaycasts = false; // レイキャストをブロックしない
            _currentCard = null; // 現在のカード情報をクリア
            Debug.Log("CardDetailView: 非表示にしました。");
            ClearButtons(); // 生成されたボタンをクリア
        }

        /// <summary>
        /// 現在表示中のカードのデッキ内枚数表示を更新します。
        /// アクティブなGameObjectの場合のみ処理を実行します。
        /// </summary>
        public void UpdateCardCount()
        {
            if (_currentCard != null && gameObject.activeInHierarchy)
            {
                int count = DeckEditorManager.Instance.GetCardCountInDeck(_currentCard.Data);
                UpdateCardCount(count); // オーバーロードされたメソッドを呼び出す
            }
        }

        /// <summary>
        /// 特定のカード（CardData）のデッキ内枚数表示を更新します。
        /// 現在表示中のカードが、更新対象のカードである場合のみ適用されます。
        /// </summary>
        /// <param name="cardData">枚数が更新されたカードのデータ。</param>
        public void UpdateCardCount(WeissCardData cardData)
        {
            if (_currentCard != null && _currentCard.Data.card_no == cardData.card_no)
            {
                int count = DeckEditorManager.Instance.GetCardCountInDeck(cardData);
                UpdateCardCount(count);
            }
        }

        /// <summary>
        /// 指定されたURLからカード画像を非同期でロードし、UIに表示します。
        /// </summary>
        /// <param name="url">ロードする画像のURL。</param>
        private IEnumerator LoadImage(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest(); // リクエストを送信し、完了を待機

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                cardImageView.sprite = sprite; // 取得した画像をSpriteとして設定
            }
            else
            {
                Debug.LogError($"画像ロード失敗 ({url}): {request.error}");
            }
        }

        /// <summary>
        /// WeissCardオブジェクトから詳細情報を抽出し、TextMeshPro向けのリッチテキスト形式で整形します。
        /// </summary>
        /// <param name="card">詳細をフォーマットするWeissCardインスタンス。</param>
        /// <returns>整形されたカード詳細テキスト。</returns>
        private string FormatCardDetails(WeissCard card)
        {
            StringBuilder sb = new StringBuilder();

            // 基本情報
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

            // 能力情報
            if (card.Abilities != null && card.Abilities.Count > 0)
            {
                sb.AppendLine("<b>--- 能力 ---</b>");
                foreach (var abilityBase in card.Abilities)
                {
                    if (abilityBase is WeissAbility ability)
                    {
                        // 能力タイプを表示 (例: 【自】)
                        sb.AppendLine($"<b>[{ability.AbilityType}]</b>");
                        // コストを表示
                        if (ability.Costs != null && ability.Costs.Count > 0)
                        {
                            sb.Append("  コスト: ");
                            foreach (var cost in ability.Costs)
                            {
                                sb.Append($"[{cost.GetDescription()}] "); // CostFactoryで生成されたコストの説明文
                            }
                            sb.AppendLine();
                        }
                        // 効果の説明を表示
                        sb.AppendLine($"  効果: {ability.Description}");
                        // TODO: 将来的にはability.Effectsを反復処理し、個々の効果を詳細に記述するべき
                    }
                    else
                    {
                        // 汎用AbilityBaseの場合の処理 (WeissAbility以外)
                        if (abilityBase.SourceCard is TCG.Core.CardBase<TCG.Weiss.WeissCardData> srcWithData)
                        {
                            sb.AppendLine($"  [汎用能力] {srcWithData.Data.name}");
                        }
                        else
                        {
                            sb.AppendLine($"  [汎用能力] (発生源不明)");
                        }
                    }
                }
            }

            // フレーバーテキスト
            if (!string.IsNullOrEmpty(card.Data.flavor_text))
            {
                sb.AppendLine();
                sb.AppendLine($"<i>{card.Data.flavor_text}</i>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 指定されたテキストとアクションを持つボタンを動的に生成し、コンテナに追加します。
        /// </summary>
        /// <param name="text">ボタンに表示するテキスト。</param>
        /// <param name="action">ボタンがクリックされたときに実行するアクション。</param>
        private void CreateButton(string text, UnityEngine.Events.UnityAction action)
        {
            if (buttonPrefab == null || buttonContainer == null)
            {
                Debug.LogError("ボタンPrefabまたはボタンコンテナが割り当てられていません。");
                return;
            }

            // Prefabからボタンをインスタンス化し、コンテナの子にする
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (button != null && buttonText != null)
            {
                buttonText.text = text; // ボタンテキストを設定
                button.onClick.AddListener(action); // クリックアクションを追加
                _createdButtons.Add(button); // 生成したボタンをリストに追加
            }
            else
            {
                Debug.LogError("Prefabからのボタン初期化に失敗しました。");
            }
        }
        
        /// <summary>
        /// デッキエディタ特有のUI要素（カード枚数テキスト、ボタンコンテナ）の表示/非表示を切り替えます。
        /// </summary>
        /// <param name="isActive">有効にする場合はtrue、無効にする場合はfalse。</param>
        private void SetDeckEditorUIActive(bool isActive)
        {
            if (cardCountText != null)
                cardCountText.gameObject.SetActive(isActive);

            if (buttonContainer != null)
                buttonContainer.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 動的に生成されたデッキ編集ボタンをすべて破棄し、リストをクリアします。
        /// </summary>
        private void ClearButtons()
        {
            foreach (var button in _createdButtons)
            {
                Destroy(button.gameObject); // ボタンのGameObjectを破棄
            }
            _createdButtons.Clear(); // リストをクリア
        }

        /// <summary>
        /// デッキ内枚数表示のテキストを更新します。
        /// </summary>
        /// <param name="count">新しい枚数。</param>
        private void UpdateCardCount(int count)
        {
            if (cardCountText != null)
            {
                cardCountText.text = $"デッキ内: {count} / 4";
            }
        }
    }
}
