using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// ストックゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IStockZone<TCard> : IZone<TCard> where TCard : Card
    {
    }
}
