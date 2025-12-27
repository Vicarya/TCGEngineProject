
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// トリガーされたが、まだ解決されていない「保留中の能力」を表すデータクラス。
    /// このインスタンスはAbilityQueueによって管理され、適切なタイミングで解決されます。
    /// </summary>
    public class PendingAbility
    {
        /// <summary>
        /// この能力の発生源であるカード。
        /// </summary>
        public WeissCard SourceCard { get; }

        /// <summary>
        /// 能力のテキスト原文。デバッグや識別のために使用されます。
        /// </summary>
        public string AbilityText { get; }

        /// <summary>
        /// この能力を解決するプレイヤー。
        /// 複数の能力が同時に誘発した場合、ヴァイスシュヴァルツのルールではターンプレイヤーから順に解決するため、
        /// どちらのプレイヤーが解決権を持つかを保持する必要があります。
        /// </summary>
        public WeissPlayer ResolvingPlayer { get; }

        /// <summary>
        /// 保留中の能力オブジェクトを初期化します。
        /// </summary>
        /// <param name="sourceCard">能力の発生源カード。</param>
        /// <param name="abilityText">能力のテキスト。</param>
        /// <param name="resolvingPlayer">能力を解決するプレイヤー。</param>
        public PendingAbility(WeissCard sourceCard, string abilityText, WeissPlayer resolvingPlayer)
        {
            SourceCard = sourceCard;
            AbilityText = abilityText;
            ResolvingPlayer = resolvingPlayer;
        }

        /// <summary>
        /// デバッグログなどで見やすいように、能力の情報を整形した文字列を返します。
        /// </summary>
        /// <returns>例: "[WS01-001] - 【自】このカードが手札から舞台に置かれた時..."</returns>
        public override string ToString()
        {
            return $"[{SourceCard.Data.CardCode}] - {AbilityText}";
        }
    }
}
