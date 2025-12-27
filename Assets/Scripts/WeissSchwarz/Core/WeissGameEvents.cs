using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツ固有のゲームイベントの種類を定義する静的クラス。
    /// これらのイベントはEventBusを通じて発行・購読されます。
    /// </summary>
    public static class WeissGameEvents
    {
        // 🌀 デッキ・クロック・リフレッシュ
        /// <summary>山札が0枚になり、リフレッシュ処理が行われる直前に発生します。</summary>
        public static readonly GameEventType DeckRefresh     = new("Weiss.DeckRefresh");
        /// <summary>リフレッシュ処理の一部として、ペナルティの1ダメージを受ける時に発生します。</summary>
        public static readonly GameEventType RefreshPenalty  = new("Weiss.RefreshPenalty");

        // 💥 バトル・アタック
        /// <summary>アタックが宣言された時に発生します。AttackDeclaredEventArgsを伴います。</summary>
        public static readonly GameEventType AttackDeclared     = new("Weiss.AttackDeclared");
        /// <summary>トリガーステップでトリガーアイコンを確認する直前に発生します。</summary>
        public static readonly GameEventType TriggerCheck       = new("Weiss.TriggerCheck");
        /// <summary>カウンターステップが開始された時に発生します。</summary>
        public static readonly GameEventType CounterStepStarted = new("Weiss.CounterStepStarted");
        /// <summary>ダメージ処理が開始される時に発生します。</summary>
        public static readonly GameEventType DamageAssigned     = new("Weiss.DamageAssigned");
        /// <summary>ダメージがキャンセルされた時に発生します。</summary>
        public static readonly GameEventType DamageCancelled    = new("Weiss.DamageCancelled");
        /// <summary>バトル（パワーの比べあい）が開始される時に発生します。</summary>
        public static readonly GameEventType BattleStarted      = new("Weiss.BattleStarted");
        /// <summary>キャラクターがバトルでリバースした時に発生します。</summary>
        public static readonly GameEventType CharacterReversed  = new("Weiss.CharacterReversed");
        /// <summary>一連のアタック（トリガーからバトル終了まで）が完了した時に発生します。</summary>
        public static readonly GameEventType AttackEnded        = new("Weiss.AttackEnded");

        // 📦 ストック／クロック操作
        /// <summary>カードがストック置場に置かれた時に発生します。</summary>
        public static readonly GameEventType CardAddedToStock   = new("Weiss.CardAddedToStock");
        /// <summary>カードがストック置場から他のゾーンに移動した時に発生します。</summary>
        public static readonly GameEventType CardRemovedFromStock = new("Weiss.CardRemovedFromStock");
        /// <summary>カードがクロック置場に置かれた時に発生します。</summary>
        public static readonly GameEventType CardAddedToClock   = new("Weiss.CardAddedToClock");
        /// <summary>カードがクロック置場から他のゾーンに移動した時に発生します。</summary>
        public static readonly GameEventType CardRemovedFromClock = new("Weiss.CardRemovedFromClock");

        // 🎯 その他
        /// <summary>アンコールが宣言された時に発生します。</summary>
        public static readonly GameEventType EncoreDeclared     = new("Weiss.EncoreDeclared");
        /// <summary>クライマックスカードがプレイされた時に発生します。</summary>
        public static readonly GameEventType ClimaxPlayed       = new("Weiss.ClimaxPlayed");
    }

    /// <summary>
    /// カードがプレイされたイベント（例: TCG.Core.BaseGameEvents.CardPlayed）において、
    /// プレイしたプレイヤーとカードの情報を運ぶためのEventArgs。
    /// </summary>
    public class CardPlayedEventArgs : System.EventArgs
    {
        /// <summary>カードをプレイしたプレイヤー。</summary>
        public WeissPlayer Player { get; }
        /// <summary>プレイされたカード。</summary>
        public WeissCard Card { get; }
        public CardPlayedEventArgs(WeissPlayer player, WeissCard card)
        {
            Player = player;
            Card = card;
        }
    }

    /// <summary>
    /// WeissGameEvents.AttackDeclared イベントにおいて、
    /// アタッカー、ディフェンダー、アタックの種類を運ぶためのEventArgs。
    /// </summary>
    public class AttackDeclaredEventArgs : System.EventArgs
    {
        /// <summary>攻撃しているキャラクター。</summary>
        public WeissCard Attacker { get; }
        /// <summary>攻撃対象となっている相手のキャラクター（ダイレクトアタックの場合はnull）。</summary>
        public WeissCard Defender { get; }
        /// <summary>アタックの種類（フロント、サイド、ダイレクト）。</summary>
        public AttackType Type { get; }
        public AttackDeclaredEventArgs(WeissCard attacker, WeissCard defender, AttackType type)
        {
            Attacker = attacker;
            Defender = defender;
            Type = type;
        }
    }
}
