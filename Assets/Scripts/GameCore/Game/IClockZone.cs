namespace TCG.Core
{
    /// <summary>
    /// クロックゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IClockZone<TCard> : IZone<TCard> where TCard : Card { }
}
