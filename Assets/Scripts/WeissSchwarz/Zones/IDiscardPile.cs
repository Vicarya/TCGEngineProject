using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 謗庫(WaitingRoom)系ゾーンを表すインターフェース。
    /// </summary>
    public interface IDiscardPile<TCard> : IZone<TCard> where TCard : Card
    {
    }

    /// <summary>
    /// Weiss用のWaitingRoom実装。
    /// </summary>
    public class WaitingRoomZone : WeissZone, IDiscardPile<WeissCard>
    {
        public WaitingRoomZone(Player owner) : base("WaitingRoom", owner, isPublic: true) { }

        public override void AddCard(WeissCard card)
        {
            cards.Insert(0, card);
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}
