namespace TCG.Weiss.Definitions
{
    /// <summary>
    /// 能力を使用するために支払う「コスト」をデータとして定義するクラス。
    /// AbilityDefinitionの一部として使用されます。
    /// この定義は、CostFactoryなどによって解釈され、ゲーム内で処理可能な
    /// 一つまたは複数のICostオブジェクトに変換されることを想定しています。
    /// 
    /// NOTE: 現状は基本的なコストを個別のプロパティで定義していますが、将来的には
    /// 「《音楽》のキャラを1枚捨てる」のような複雑なコストに対応するため、
    /// より汎用的なリスト形式のデータ構造に拡張される可能性があります。
    /// </summary>
    public class CostDefinition
    {
        /// <summary>
        /// 支払うストックの枚数を指定します。
        /// </summary>
        public int Stock { get; set; } = 0;

        /// <summary>
        /// 捨てる手札の枚数を指定します。（対象は任意）
        /// </summary>
        public int Discard { get; set; } = 0;

        /// <summary>
        /// 能力の発生源であるカード自身をレストするかどうかを指定します。
        /// </summary>
        public bool RestSelf { get; set; } = false;

        /// <summary>
        /// 能力の発生源であるカード自身を思い出置場に送るかどうかを指定します。
        /// </summary>
        public bool SendSelfToMemory { get; set; } = false;
    }
}
