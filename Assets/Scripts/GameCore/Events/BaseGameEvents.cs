namespace TCG.Core {
    /// <summary>
    /// 全てのTCGで共通して利用可能性のある、基本的なゲームイベントの種類を定義する静的クラス。
    /// イベント駆動アーキテクチャの中心となり、ゲーム内で発生した出来事を通知するために使用されます。
    /// </summary>
    public static class BaseGameEvents
    {
        // --- 🎴 カード移動・状態変化 ---

        /// <summary>カードが山札から引かれたときに発生します。</summary>
        public static readonly GameEventType CardDrawn      = new("CardDrawn");
        /// <summary>カードが手札から場に出されたときに発生します。</summary>
        public static readonly GameEventType CardPlayed     = new("CardPlayed");
        /// <summary>カードが手札から捨てられたときに発生します。</summary>
        public static readonly GameEventType CardDiscarded  = new("CardDiscarded");
        /// <summary>カードがあるゾーンから別のゾーンへ移動したときに発生します。</summary>
        public static readonly GameEventType CardMoved      = new("CardMoved");
        /// <summary>カードが破壊された（場から控え室などに置かれた）ときに発生します。</summary>
        public static readonly GameEventType CardDestroyed  = new("CardDestroyed");
        /// <summary>カードが表向きまたは裏向きにされたときに発生します。</summary>
        public static readonly GameEventType CardFlipped    = new("CardFlipped");
        /// <summary>カードがタップ（横向きに）されたときに発生します。</summary>
        public static readonly GameEventType CardTapped     = new("CardTapped");
        /// <summary>カードがアンタップ（縦向きに）されたときに発生します。</summary>
        public static readonly GameEventType CardUntapped   = new("CardUntapped");

        // --- 🧑 プレイヤー行動 ---

        /// <summary>新しいターンが開始されたときに発生します。</summary>
        public static readonly GameEventType TurnStarted    = new("TurnStarted");
        /// <summary>ターンが終了したときに発生します。</summary>
        public static readonly GameEventType TurnEnded      = new("TurnEnded");
        /// <summary>新しいフェーズが開始されたときに発生します。</summary>
        public static readonly GameEventType PhaseStarted   = new("PhaseStarted");
        /// <summary>フェーズが終了したときに発生します。</summary>
        public static readonly GameEventType PhaseEnded     = new("PhaseEnded");
        /// <summary>プレイヤーが敗北したときに発生します。</summary>
        public static readonly GameEventType PlayerLost     = new("PlayerLost");

        // --- ✨ アビリティ・効果 ---

        /// <summary>プレイヤーが能力の使用を宣言したときに発生します。</summary>
        public static readonly GameEventType AbilityDeclared  = new("AbilityDeclared");
        /// <summary>能力のコストが支払われたときに発生します。</summary>
        public static readonly GameEventType AbilityCostPaid  = new("AbilityCostPaid");
        /// <summary>能力の効果が解決されたときに発生します。</summary>
        public static readonly GameEventType AbilityResolved  = new("AbilityResolved");
        /// <summary>能力の使用がキャンセルされたときに発生します。</summary>
        public static readonly GameEventType AbilityCancelled = new("AbilityCancelled");
    }
}