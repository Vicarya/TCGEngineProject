namespace TCG.Core
{
    /// <summary>
    /// クライマックスゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IClimaxZone<TCard> : IZone<TCard> where TCard : Card { }
}
