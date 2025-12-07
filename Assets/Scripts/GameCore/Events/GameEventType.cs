namespace TCG.Core {
    /// <summary>
    /// ゲームイベントの種類を一意に識別するための型。
    /// 文字列による比較を避け、より効率的な整数インデックスでの識別に利用します。
    /// </summary>
    public class GameEventType
    {
        /// <summary>
        /// 新しいイベントタイプが作成されるたびにインクリメントされ、ユニークなIndexを払い出すための静的カウンター。
        /// </summary>
        private static int nextIndex = 0;

        /// <summary>
        /// イベントタイプを一意に識別するための整数インデックス。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// イベントタイプの名前（デバッグ用）。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// GameEventTypeクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">イベントタイプの名前。</param>
        public GameEventType(string name)
        {
            Index = nextIndex++;
            Name = name;
        }
    }
}