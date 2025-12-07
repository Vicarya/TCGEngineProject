namespace TCG.Core
{
    /// <summary>
    /// ゲーム固有の静的なカードデータ（CardData）を持つ、ジェネリックなカードの抽象基底クラス。
    /// これを継承して、各ゲーム（例：ヴァイスシュヴァルツ、ポケモンカードゲーム）の具体的なカードクラスを実装します。
    /// </summary>
    /// <typeparam name="TData">このカードが保持する静的データの型。CardDataの派生クラスである必要があります。</typeparam>
    public abstract class CardBase<TData> : Card where TData : CardData
    {
        /// <summary>
        /// このカードの静的なカードデータを取得します。（例：カード名、パワー、テキストなど）
        /// </summary>
        public TData Data { get; private set; }

        /// <summary>
        /// CardBaseクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="data">このカードが使用する静的カードデータ。</param>
        /// <param name="owner">このカードの所有者。</param>
        protected CardBase(TData data, Player owner)
        {
            Data = data;
            Owner = owner;
            IsTapped = false;
            IsFaceUp = true;
        }
    }
}
