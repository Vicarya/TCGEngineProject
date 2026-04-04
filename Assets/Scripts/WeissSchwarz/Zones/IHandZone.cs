using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 手札ゾーンを表すインターフェース。
    /// </summary>
    public interface IHandZone<TCard> : IZone<TCard> where TCard : Card
    {
    }

    /// <summary>
    /// Weiss用の手札ゾーン実装。
    /// </summary>
    public class HandZone : WeissZone, IHandZone<WeissCard>
    {
        public HandZone(Player owner) : base("Hand", owner, isPublic: false) { }
    }
}
