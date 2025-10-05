using TCG.Core;

namespace TCG.Weiss {
    public static class WeissGameEvents
    {
        // 🌀 デッキ・クロック・リフレッシュ
        public static readonly GameEventType DeckRefresh     = new("Weiss.DeckRefresh");
        public static readonly GameEventType RefreshPenalty  = new("Weiss.RefreshPenalty");

        // 💥 バトル・アタック (引数は AttackPhaseEventArgs を想定)
        public static readonly GameEventType AttackDeclared     = new("Weiss.AttackDeclared");
        public static readonly GameEventType TriggerCheck       = new("Weiss.TriggerCheck");
        public static readonly GameEventType CounterStepStarted = new("Weiss.CounterStepStarted");
        public static readonly GameEventType DamageAssigned     = new("Weiss.DamageAssigned");
        public static readonly GameEventType DamageCancelled    = new("Weiss.DamageCancelled");
        public static readonly GameEventType BattleStarted      = new("Weiss.BattleStarted");
        public static readonly GameEventType CharacterReversed  = new("Weiss.CharacterReversed");
        public static readonly GameEventType AttackEnded        = new("Weiss.AttackEnded");

        // 📦 ストック／クロック操作
        public static readonly GameEventType CardAddedToStock   = new("Weiss.CardAddedToStock");
        public static readonly GameEventType CardRemovedFromStock = new("Weiss.CardRemovedFromStock");
        public static readonly GameEventType CardAddedToClock   = new("Weiss.CardAddedToClock");
        public static readonly GameEventType CardRemovedFromClock = new("Weiss.CardRemovedFromClock");

        // 🎯 その他
        public static readonly GameEventType EncoreDeclared     = new("Weiss.EncoreDeclared");
        public static readonly GameEventType ClimaxPlayed       = new("Weiss.ClimaxPlayed");
    }
}