using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// クロックゾーンを表すインターフェース。
    /// </summary>
    public interface IClockZone<TCard> : IZone<TCard> where TCard : Card
    {
    }

    /// <summary>
    /// Weiss用のクロックゾーン実装。
    /// </summary>
    public class ClockZone : WeissZone, IClockZone<WeissCard>
    {
        public LevelZone LevelZone { get; }
        public WaitingRoomZone WaitingRoom { get; }

        public ClockZone(Player owner, LevelZone levelZone, WaitingRoomZone waitingRoom)
            : base("Clock", owner, isPublic: true)
        {
            Owner = owner;
            LevelZone = levelZone;
            WaitingRoom = waitingRoom;
        }

        public override void AddCard(WeissCard card)
        {
            cards.Add(card);
        }

        public void RemoveCards(IEnumerable<WeissCard> cardsToRemove)
        {
            foreach (var card in cardsToRemove.ToList())
            {
                cards.Remove(card);
            }
        }

        public WeissCard RemoveTopCard()
        {
            if (!cards.Any()) throw new InvalidOperationException("ClockZone is empty");
            var top = cards[^1];
            cards.RemoveAt(cards.Count - 1);
            return top;
        }
    }
}
