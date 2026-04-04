using System;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// クライマックスゾーンを表すインターフェース。
    /// </summary>
    public interface IClimaxZone<TCard> : IZone<TCard> where TCard : Card
    {
    }

    /// <summary>
    /// Weiss用のクライマックスゾーン実装。
    /// </summary>
    public class ClimaxZone : WeissZone, IClimaxZone<WeissCard>
    {
        public ClimaxZone(Player owner) : base("Climax", owner, isPublic: true) { }

        public override void AddCard(WeissCard card)
        {
            if (((WeissCardData)card.Data).CardType != WeissCardType.Climax.ToString())
                throw new InvalidOperationException("ClimaxZone can only hold climax cards");
            if (cards.Count >= 1)
                throw new InvalidOperationException("ClimaxZone can only hold 1 card");
            cards.Add(card);
        }

        public WeissCard RemoveTopCard()
        {
            if (!cards.Any()) throw new InvalidOperationException("ClimaxZone is empty");
            var c = cards[0];
            cards.Clear();
            return c;
        }
    }
}
