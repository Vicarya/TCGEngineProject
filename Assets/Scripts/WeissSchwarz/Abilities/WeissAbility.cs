using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 汎用的なAbilityBaseを継承した、ヴァイスシュヴァルツのカード能力を表す具体的なクラス。
    /// AbilityFactoryによって能力テキストから生成され、能力の種類（自動、起動、常時）や
    /// 効果テキストといった、ヴァイスシュヴァルツ固有の情報を保持します。
    /// </summary>
    public class WeissAbility : AbilityBase
    {
        /// <summary>
        /// 能力の種類（【自】自動、【起】起動、【永】常時）を取得または設定します。
        /// </summary>
        public AbilityType AbilityType { get; set; }

        /// <summary>
        /// 能力のコストやトリガー部分を除いた、効果を説明するテキスト部分。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 新しいWeissAbilityインスタンスを初期化します。
        /// </summary>
        /// <param name="source">この能力の発生源となるカード。</param>
        public WeissAbility(Card source) : base(source)
        {
        }
    }

    /// <summary>
    /// ヴァイスシュヴァルツの能力種別を定義します。
    /// </summary>
    public enum AbilityType
    {
        /// <summary>
        /// 【自】自動能力。特定の条件が満たされたときに自動で誘発します。
        /// </summary>
        Auto,
        /// <summary>
        /// 【起】起動能力。メインフェイズにコストを支払って任意で使用できます。
        /// </summary>
        Activated,
        /// <summary>
        /// 【永】常時能力。特定のゾーンにある限り、常に効果が適用されます。
        /// </summary>
        Continuous
    }
}
