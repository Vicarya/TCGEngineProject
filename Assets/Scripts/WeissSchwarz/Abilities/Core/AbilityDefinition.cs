using System.Collections.Generic;

// アビリティの種別
public enum AbilityType
{
    Auto,       // 自動能力
    Act,        // 起動能力
    Cont        // 常時能力
}

public class AbilityDefinition 
{
    public string Name { get; set; }              // "集中", "助太刀" etc
    public AbilityType Type { get; set; }         // Auto / Activated / Continuous
    // どのイベントやフェーズで発動できるかを定義
    // 例: 「Phase == Main」「Event == CardPlayed」「Self.Reversed == true」
    public ConditionDefinition Condition { get; set; }
    // 支払う必要があるコストを定義
    // 例: 「Stock = 1」「Discard = 1」「Rest = Self」
    public CostDefinition Cost { get; set; }
    // 実際の効果を定義
    // 例: 「RevealTop(4)」「Draw(1)」「ModifyPower(+2000, ally)」
    public List<EffectDefinition> Effects { get; set; }
}

