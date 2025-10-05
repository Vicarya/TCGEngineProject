using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// 山札ゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IDeckZone<TCard> : IZone<TCard> where TCard : Card
    {
        IReadOnlyList<TCard> Peek(int count);
        void Shuffle();
        TCard DrawTop();
    }
}
