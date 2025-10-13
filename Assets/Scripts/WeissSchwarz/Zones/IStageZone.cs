using TCG.Core;
using System.Collections.Generic;

namespace TCG.Weiss
{
    public interface IStageSlot<TCard> : IZone<TCard> where TCard : Card
    {
        TCard Current { get; }
        void PlaceCharacter(TCard card);
    }

    /// <summary>
    /// 舞台ゾーンを示すマーカーインターフェース
    /// </summary>
    public interface IStageZone<TCard> : IZone<TCard> where TCard : Card
    {
        IEnumerable<IStageSlot<TCard>> Slots { get; }
    }
}
