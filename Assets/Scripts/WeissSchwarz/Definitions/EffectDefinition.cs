namespace TCG.Weiss.Definitions
{
    /// <summary>
    /// 能力の効果の種類を定義します。
    /// </summary>
    public enum EffectKind
    {
        /// <summary>カードを引く。</summary>
        Draw,
        /// <summary>山札の上からカードを公開する。</summary>
        RevealTop,
        /// <summary>山札からカードを探す。</summary>
        SearchDeck,
        /// <summary>パワーを増減させる。</summary>
        PowerModifier,
        /// <summary>カードを特定のゾーンに移動させる。</summary>
        MoveToZone,
        /// <summary>クロックからカードを回復する（ヒール）。</summary>
        Heal,
        /// <summary>相手にダメージを与える。</summary>
        DealDamage,
        /// <summary>上記に分類されない、固有のIDで識別されるカスタム効果。</summary>
        Custom
    }

    /// <summary>
    /// 能力の「効果」そのものをデータとして定義するクラス。
    /// AbilityDefinitionの一部として使用されます。
    /// この定義は、EffectFactoryなどによって解釈され、ゲーム内で実行可能な
    /// 一つまたは複数のIEffectオブジェクトに変換されることを想定しています。
    /// </summary>
    public class EffectDefinition
    {
        /// <summary>
        /// この効果の種類（ドロー、パワー修正など）。
        /// </summary>
        public EffectKind Kind { get; set; }

        /// <summary>
        /// 効果の量（ドローする枚数、与えるダメージ量など）。
        /// </summary>
        public int Amount { get; set; } = 1;

        /// <summary>
        /// 効果の対象を指定します。例：「Self」（自身）、「Opponent」（相手）、「Ally」（味方）、「All」（すべて）。
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 効果が関連するゾーンを指定します。例：「Deck」（山札）、「WaitingRoom」（控え室）。
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// パワーを修正する場合の、その修正値を指定します。例：+2000, -1000。
        /// </summary>
        public int PowerValue { get; set; }

        /// <summary>
        /// カードを移動させる際に、表向きで置くかどうかを指定します。（例：思い出に裏向きで置く）。
        /// </summary>
        public bool FaceUp { get; set; } = true;

        /// <summary>
        /// KindがCustomの場合に、具体的な効果の処理を識別するためのユニークなID。
        /// </summary>
        public string CustomId { get; set; }
    }
}
