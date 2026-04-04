using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// ストックゾーンを表すインターフェース。
    /// </summary>
    public interface IStockZone<TCard> : IZone<TCard> where TCard : Card
    {
        TCard RemoveTopCard();
    }

    /// <summary>
    /// Weiss用のストックゾーン実装。
    /// </summary>
    public class StockZone : WeissZone, IStockZone<WeissCard>
    {
        public StockZone(Player owner) : base("Stock", owner, isPublic: false) { }

        public override void AddCard(WeissCard card) => cards.Insert(0, card);

        public WeissCard RemoveTopCard()
        {
            if (cards.Count == 0) return null;
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
    }
}
