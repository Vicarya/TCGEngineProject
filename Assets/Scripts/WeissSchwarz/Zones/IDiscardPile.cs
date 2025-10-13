using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 捨て札置き場（控え室）を示すマーカーインターフェース
    /// </summary>
    public interface IDiscardPile<TCard> : IZone<TCard> where TCard : Card
    {
    }
}
