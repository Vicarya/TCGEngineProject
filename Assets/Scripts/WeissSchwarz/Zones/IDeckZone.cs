using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// デッキゾーンを表すインターフェース。
    /// </summary>
    public interface IDeckZone<TCard> : IZone<TCard> where TCard : Card
    {
        TCard DrawTop();
        TCard PeekTop();
        void Shuffle();
    }

    /// <summary>
    /// Weiss用のデッキゾーン実装。
    /// </summary>
    public class DeckZone : DeckZoneBase<WeissCard>, IDeckZone<WeissCard>
    {
        public DeckZone(Player owner) : base("Deck", owner) { }

        public void AddTop(WeissCard card) => cards.Insert(0, card);
        public void AddBottom(WeissCard card) => base.AddCard(card);

        public IReadOnlyList<WeissCard> Reveal(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (cards.Count < count) throw new InvalidOperationException("Not enough cards in deck");
            var revealed = cards.Take(count).ToList();
            cards.RemoveRange(0, count);
            return revealed;
        }

        public void RefreshFrom(WaitingRoomZone waitingRoom, ClockZone clock)
        {
            if (cards.Count > 0) return;

            foreach (var card in waitingRoom.Cards)
            {
                AddCard(card);
            }
            waitingRoom.Clear();

            Shuffle();

            var refreshPenalty = DrawTop();
            if (refreshPenalty != null)
            {
                clock.AddCard(refreshPenalty);
            }
        }
    }
}
