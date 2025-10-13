using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 手札ゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IHandZone<TCard> : IZone<TCard> where TCard : Card
    {
    }
}
