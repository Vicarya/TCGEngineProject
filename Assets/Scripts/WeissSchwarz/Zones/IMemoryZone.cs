using System;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// メモリーゾーンを表すインターフェース。
    /// </summary>
    public interface IMemoryZone<TCard> : IZone<TCard> where TCard : Card
    {
        IReadOnlyList<(TCard Card, bool FaceUp)> MemoryCards { get; }
        (TCard Card, bool FaceUp) RemoveSpecificCard(TCard card);
    }

    /// <summary>
    /// Weiss用のメモリーゾーン実装。
    /// </summary>
    public class MemoryZone : WeissZone, IMemoryZone<WeissCard>
    {
        private readonly List<(WeissCard Card, bool FaceUp)> memoryCards = new();

        public MemoryZone(Player owner) : base("Memory", owner, isPublic: true) { }

        public IReadOnlyList<(WeissCard Card, bool FaceUp)> MemoryCards => memoryCards;
        public int Count => memoryCards.Count;

        public void AddCard(WeissCard card, bool faceUp = true)
        {
            memoryCards.Insert(0, (card, faceUp));
        }

        public (WeissCard Card, bool FaceUp) RemoveSpecificCard(WeissCard card)
        {
            var idx = memoryCards.FindIndex(c => c.Card == card);
            if (idx < 0) throw new InvalidOperationException("Card not found in MemoryZone");

            var tuple = memoryCards[idx];
            memoryCards.RemoveAt(idx);
            return tuple;
        }
    }
}
