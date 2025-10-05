public class ConditionDefinition
{
    public string Event { get; set; } // "OnAttack", "OnPlay", "OnReverse" など
    public string Phase { get; set; } // "MainPhase" など
    public string SelfState { get; set; } // "Reversed" "Rested" など
    public string Zone { get; set; } // "Stage", "Hand" など
    public string Filter { get; set; } // "Trait:Music" "Level>=2" 等
}
