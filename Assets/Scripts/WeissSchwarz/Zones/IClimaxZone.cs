using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// クライマックスゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IClimaxZone<TCard> : IZone<TCard> where TCard : Card
    {
    }
}