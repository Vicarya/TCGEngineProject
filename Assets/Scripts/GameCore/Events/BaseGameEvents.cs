namespace TCG.Core {
    public static class BaseGameEvents
    {
        // 🎴 カード移動・状態変化
        public static readonly GameEventType CardDrawn      = new("CardDrawn");
        public static readonly GameEventType CardPlayed     = new("CardPlayed");
        public static readonly GameEventType CardDiscarded  = new("CardDiscarded");
        public static readonly GameEventType CardMoved      = new("CardMoved");
        public static readonly GameEventType CardDestroyed  = new("CardDestroyed");
        public static readonly GameEventType CardFlipped    = new("CardFlipped");
        public static readonly GameEventType CardTapped     = new("CardTapped");
        public static readonly GameEventType CardUntapped   = new("CardUntapped");

        // 🧑 プレイヤー行動
        public static readonly GameEventType TurnStarted    = new("TurnStarted");
        public static readonly GameEventType TurnEnded      = new("TurnEnded");
        public static readonly GameEventType PhaseStarted   = new("PhaseStarted");
        public static readonly GameEventType PhaseEnded     = new("PhaseEnded");
        public static readonly GameEventType PlayerLost     = new("PlayerLost");

        // ✨ アビリティ・効果
        public static readonly GameEventType AbilityDeclared  = new("AbilityDeclared");
        public static readonly GameEventType AbilityCostPaid  = new("AbilityCostPaid");
        public static readonly GameEventType AbilityResolved  = new("AbilityResolved");
        public static readonly GameEventType AbilityCancelled = new("AbilityCancelled");
    }
}