using System.Collections.Generic;
using TCG.Weiss.Definitions; // CostDefinition, EffectDefinitionなどが必要なため追加

namespace TCG.Weiss.Abilities
{
    // NOTE: このAbilityType enumは `WeissAbility.cs` 内のenumと役割が重複しています。
    // 将来的にはどちらかに統一することが望ましいです。
    /// <summary>
    /// アビリティの種別を定義します。
    /// </summary>
    public enum AbilityType
    {
        Auto,       // 自動能力 【自】
        Act,        // 起動能力 【起】
        Cont        // 常時能力 【永】
    }

    /// <summary>
    /// ヴァイスシュヴァルツのカード能力を「データ」として定義するための中核的なクラス。
    /// このクラスは、JSONなどのデータ形式からデシリアライズされることを想定しています。
    /// これにより、プログラマーでないゲームデザイナーでも、データファイルを編集するだけで
    /// 新しいカード能力を設計・実装できるようになります（データ駆動設計）。
    ///
    /// このAbilityDefinitionオブジェクトは、最終的にAbilityFactoryによって解釈され、
    /// ゲーム内で実行可能なWeissAbilityオブジェクトに変換されます。
    /// </summary>
    public class AbilityDefinition
    {
        /// <summary>
        /// 能力のキーワードや通称（例：「集中」「助太刀」など）。分類や識別に使われます。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 能力の種類（自動、起動、常時）。
        /// </summary>
        public AbilityType Type { get; set; }

        /// <summary>
        /// 能力がいつ誘発・適用されるかの「条件」をデータとして定義します。
        /// 例：「Phase == Main」「Event == CardPlayed」「Self.Reversed == true」
        /// </summary>
        public ConditionDefinition Condition { get; set; }

        /// <summary>
        /// 能力を使用するために支払う「コスト」をデータとして定義します。
        /// 例：「Stock = 1」「Discard = 1」「Rest = Self」
        /// </summary>
        public CostDefinition Cost { get; set; }

        /// <summary>
        /// 能力が解決されたときに実行される実際の「効果」をデータとして定義します。
        /// 例：「RevealTop(4)」「Draw(1)」「ModifyPower(+2000, ally)」
        /// </summary>
        public List<EffectDefinition> Effects { get; set; }
    }
}
