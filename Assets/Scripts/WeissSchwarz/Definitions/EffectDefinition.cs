public enum EffectKind
{
    Draw,
    RevealTop,
    SearchDeck,
    PowerModifier,
    MoveToZone,
    Heal,
    DealDamage,
    Custom // 特殊効果はここで
}

public class EffectDefinition
{
    public EffectKind Kind { get; set; }
    public int Amount { get; set; } = 1; // 枚数やダメージ量など
    public string Target { get; set; }   // "Self", "Opponent", "Ally", "All"
    public string Zone { get; set; }     // "Deck", "WaitingRoom", "Stage"
    public int PowerValue { get; set; }  // パワー修正値
    public bool FaceUp { get; set; } = true; // メモリーなどで裏向き
    public string CustomId { get; set; } // カスタムエフェクトの識別子
}
