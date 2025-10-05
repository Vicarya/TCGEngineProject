namespace TCG.Core
{
    /// <summary>
    /// 控え室（墓地）を示すマーカーインターフェース
    /// </summary>
    public interface IDiscardPile<TCard> : IZone<TCard> where TCard : Card
    {
        void Clear();
    }
}
