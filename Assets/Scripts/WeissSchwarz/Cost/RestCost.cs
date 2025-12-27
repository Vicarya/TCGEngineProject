using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 「カードをレストする」というコストを表現・処理するクラス。
    /// TCG.Core.ICostインターフェースを実装しています。
    /// </summary>
    public class RestCost : ICost
    {
        /// <summary>
        /// レストする対象が、この能力の発生源であるカード自身であるかどうかを示します。
        /// trueの場合、「【起】 このカードを【レスト】する」のようなコストを表します。
        /// </summary>
        public bool IsSourceCard { get; }

        /// <summary>
        /// 新しいレストコストのインスタンスを初期化します。
        /// </summary>
        /// <param name="isSourceCard">コストとして能力の発生源自身をレストする場合はtrue。</param>
        public RestCost(bool isSourceCard = false)
        {
            IsSourceCard = isSourceCard;
        }

        /// <summary>
        /// プレイヤーがコストを支払える状態にあるか（＝対象がレスト可能か）を確認します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>支払い可能な場合はtrue、そうでない場合はfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            if (IsSourceCard)
            {
                // コストが能力元自身のレストである場合、そのカードがまだレストされていなければ支払い可能。
                return source != null && !source.IsRested;
            }
            // TODO: 能力元以外のカードをレストするコスト（例：「あなたのキャラを1枚【レスト】する」）の
            // 支払い可否チェックロジックを実装する必要があります。
            return false;
        }

        /// <summary>
        /// 実際にカードをレストしてコストを支払います。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            if (IsSourceCard && source != null)
            {
                // NOTE: WeissCardのRest()メソッドを呼ぶのがより望ましいが、
                // ICostはジェネリックなCard型しか知らないため、基底のプロパティを直接操作している。
                source.SetRested(true);
                source.IsTapped = true;
            }
            // TODO: 能力元以外のカードをレストするコストの支払い処理を実装する必要があります。
        }

        /// <summary>
        /// UI表示用のコスト説明文を返します。
        /// </summary>
        /// <returns>コストを説明する文字列。</returns>
        public string GetDescription()
        {
            if (IsSourceCard)
            {
                return "このカードを【レスト】する";
            }
            // TODO: より具体的な説明文（例：「あなたの《音楽》のキャラを1枚【レスト】する」）を生成できるようにする。
            return "指定されたカードを【レスト】する";
        }
    }
}
