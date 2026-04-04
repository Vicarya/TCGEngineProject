using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// レベルゾーンの公開契約。
    /// </summary>
    public interface ILevelZone<TCard> : IZone<TCard> where TCard : Card
    {
        IReadOnlyList<(TCard Card, bool FaceUp)> LevelCards { get; }
    }

    /// <summary>
    /// Weiss用のレベルゾーン実装。
    /// </summary>
    public class LevelZone : WeissZone, ILevelZone<WeissCard>
    {
        private readonly List<(WeissCard Card, bool FaceUp)> levelCards = new();

        public LevelZone(Player owner) : base("Level", owner, isPublic: true) { }

        public void AddCard(WeissCard card, bool faceUp = true)
        {
            levelCards.Add((card, faceUp));
        }

        public IReadOnlyList<(WeissCard Card, bool FaceUp)> LevelCards => levelCards;
        public int Count => levelCards.Count;
    }
}
