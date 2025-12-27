namespace TCG.Weiss.Definitions
{
    /// <summary>
    /// 能力が誘発・適用されるための「条件」をデータとして定義するクラス。
    /// AbilityDefinitionの一部として使用されます。
    /// この定義は、ルールエンジンによって解釈され、実際のゲーム状態と比較されることで、
    /// 能力が使えるかどうかを判断するために利用されます。
    /// </summary>
    public class ConditionDefinition
    {
        /// <summary>
        /// トリガーとなるゲームイベントを指定します。
        /// 例：「OnAttack」（アタックした時）、「OnPlay」（プレイされた時）、「OnReverse」（リバースした時）
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 能力が有効になる、あるいは使用可能になるゲームフェーズを指定します。
        /// 例：「MainPhase」（メインフェイズ）
        /// </summary>
        public string Phase { get; set; }

        /// <summary>
        /// 能力を持つカード自身の状態を指定します。
        /// 例：「Reversed」（リバース状態）、「Rested」（レスト状態）
        /// </summary>
        public string SelfState { get; set; }

        /// <summary>
        /// 能力を持つカードが存在するべきゾーンを指定します。
        /// 例：「Stage」（舞台）、「Hand」（手札）
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// その他のより複雑な条件を、カスタム文字列で指定します。
        /// この文字列は、ルールエンジン内の特定のパーサーによって解釈されます。
        /// 例：「Trait:Music」（特徴に「音楽」を持つ）、「Level>=2」（レベルが2以上）
        /// </summary>
        public string Filter { get; set; }
    }
}
