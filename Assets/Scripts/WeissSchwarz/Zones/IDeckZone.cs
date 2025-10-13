using System.Collections.Generic;

using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 山札ゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IDeckZone<TCard> : IZone<TCard> where TCard : Card
    {
        TCard DrawTop();
        TCard PeekTop();
        void Shuffle();
    }
}
