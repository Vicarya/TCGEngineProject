namespace TCG.Weiss {
    public enum WeissCardType { Character, Event, Climax }
    public enum WeissColor { Red, Blue, Yellow, Green, Black, White, Colorless }
    public enum TriggerIcon { None, SoulPlus, Draw /*, ???? */ } // ???? = placeholder - customize per icon sets

    public enum MainPhaseAction
    {
        PlayCard,
        UseAbility,
        EndPhase
    }
}

namespace TCG.Weiss
{
    public enum AttackType
    {
        Front, Side, Direct
    }
}
