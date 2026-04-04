using System;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 解決中の一時ゾーンを表すインターフェース。
    /// </summary>
    public interface IResolutionZone<TCard> : IZone<TCard> where TCard : Card
    {
        TCard RemoveTopCard();
    }

    /// <summary>
    /// Weiss用の解決ゾーン実装。
    /// </summary>
    public class ResolutionZone : WeissZone, IResolutionZone<WeissCard>
    {
        public ResolutionZone(Player owner) : base("Resolution", owner, isPublic: true) { }

        public WeissCard RemoveTopCard()
        {
            if (!cards.Any()) throw new InvalidOperationException("ResolutionZone is empty");
            var top = cards[^1];
            cards.RemoveAt(cards.Count - 1);
            return top;
        }
    }
}
