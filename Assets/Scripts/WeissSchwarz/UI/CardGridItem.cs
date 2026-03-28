using UnityEngine;
using UnityEngine.UI;
using TCG.Weiss;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// デッキエディタのグリッドに表示される各カードUI要素を管理するコンポーネント。
    /// 自身でImageとButtonコンポーネントを必須とし、初期化時にそれらを設定する。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardGridItem : MonoBehaviour
    {
        private Image _cardImage;
        private Button _button;
        private WeissCardData _cardData;

        /// <summary>
        /// このコンポーネントがアタッチされたGameObjectに必要なコンポーネントを自動で追加・設定します。
        /// </summary>
        private void Awake()
        {
            // Imageコンポーネントを取得、なければ追加
            _cardImage = gameObject.GetComponent<Image>();
            if (_cardImage == null)
            {
                _cardImage = gameObject.AddComponent<Image>();
            }

            // Buttonコンポーネントを取得、なければ追加
            _button = gameObject.GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
            }
        }

        /// <summary>
        /// カードデータでこのUI要素を初期化します。
        /// </summary>
        /// <param name="cardData">表示するカードのデータ。</param>
        public void Initialize(WeissCardData cardData)
        {
            _cardData = cardData;
            gameObject.name = _cardData.CardCode;

            // ボタンのクリックイベントを設定
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnCardClick);

            // 画像の非同期読み込みを開始
            // ImageLoader.cs がプロジェクトに存在することを想定
            // 存在しない場合は、この部分をコメントアウトまたは実装してください。
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ImageLoader.LoadImage(_cardData.ImagePath, _cardImage));
            }
        }

        /// <summary>
        /// カードがクリックされたときに呼び出されます。
        /// </summary>
        private void OnCardClick()
        {
            if (_cardData != null)
            {
                // DeckEditorManagerのシングルトンインスタンス経由で詳細表示を要求
                DeckEditorManager.Instance.ShowCardDetail(_cardData);
            }
        }
    }
}
