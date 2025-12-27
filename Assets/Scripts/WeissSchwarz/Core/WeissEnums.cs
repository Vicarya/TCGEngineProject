namespace TCG.Weiss 
{
    /// <summary>
    /// ヴァイスシュヴァルツのカード種類を定義します。
    /// </summary>
    public enum WeissCardType { 
        Character,  // キャラクターカード
        Event,      // イベントカード
        Climax      // クライマックスカード
    }

    /// <summary>
    /// ヴァイスシュヴァルツのカードの色を定義します。
    /// </summary>
    public enum WeissColor { 
        Yellow,     // 黄
        Green,      // 緑
        Red,        // 赤
        Blue,       // 青
        Black,      // 黒 (シュヴァルツサイド)
        White,      // 白 (ヴァイスサイド)
        Colorless   // 無色
    }
    
    /// <summary>
    /// ヴァイスシュヴァルツのトリガーアイコンを定義します。
    /// </summary>
    public enum TriggerIcon { 
        None,       // なし
        SoulPlus,   // ソウルがプラスされるアイコン（例: Soul+1, 2 Soul）
        Draw,       // ドローアイコン（風）
        Bounce,     // バウンスアイコン（扉）
        Salvage,    // サルベージアイコン（袋）
        Stock,      // ストックアイコン（本）
        Gate,       // ゲートアイコン（門）
        Treasure    // トレジャーアイコン（宝）
    }

    /// <summary>
    /// メインフェイズにプレイヤーが選択できる行動を定義します。
    /// </summary>
    public enum MainPhaseAction
    {
        PlayCard,   // カードをプレイする
        UseAbility, // 起動能力を使用する
        EndPhase    // メインフェイズを終了する
    }

    /// <summary>
    /// アタックの種類を定義します。
    /// </summary>
    public enum AttackType
    {
        Front,  // 正面のアタック対象キャラにフロントアタック
        Side,   // 正面のアタック対象キャラにサイドアタック
        Direct  // プレイヤーにダイレクトアタック
    }

    /// <summary>
    /// アンコールの種類を定義します。
    /// </summary>
    public enum EncoreChoice
    {
        None,       // アンコールしない
        Standard,   // 3コストを支払って行う通常のアンコール
        Special     // カード自身の能力（例：【自】アンコール［手札のキャラを1枚控え室に置く］）で行うアンコール
    }
}
