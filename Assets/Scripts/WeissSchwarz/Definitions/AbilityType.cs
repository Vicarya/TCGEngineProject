namespace TCG.Weiss.Definitions
{
    /// <summary>
    /// ヴァイスシュヴァルツのカードが持つ能力の種類を定義します。
    /// データ駆動設計における能力定義の基礎となります。
    /// </summary>
    public enum AbilityType
    {
        /// <summary>
        /// 【自】(自動能力): 「～した時」「～フェイズの始めに」など、特定のイベント発生時に自動的に誘発する能力。
        /// </summary>
        Auto,

        /// <summary>
        /// 【起】(起動能力): メインフェイズ中にプレイヤーが任意でコストを支払って使用できる能力。
        /// </summary>
        Activated,

        /// <summary>
        /// 【永】(常時能力): このカードが特定のゾーンにある限り、常に効果が適用され続ける能力。
        /// </summary>
        Continuous
    }
}
