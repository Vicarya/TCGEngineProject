using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    public interface IStageSlot<TCard> : IZone<TCard> where TCard : Card
    {
        TCard Current { get; }
        void PlaceCharacter(TCard card);
    }

    /// <summary>
    /// ステージゾーンを表すインターフェース。
    /// </summary>
    public interface IStageZone<TCard> : IZone<TCard> where TCard : Card
    {
        IEnumerable<IStageSlot<TCard>> Slots { get; }
    }

    /// <summary>
    /// カードの向き状態。
    /// </summary>
    public enum CardOrientation
    {
        Upright,
        Sideways,
        Reversed
    }

    /// <summary>
    /// Weiss用のステージゾーン実装。
    /// </summary>
    public class StageZone : WeissZone, IStageZone<WeissCard>
    {
        public StageSlot FrontLeft { get; }
        public StageSlot FrontCenter { get; }
        public StageSlot FrontRight { get; }
        public StageSlot BackLeft { get; }
        public StageSlot BackRight { get; }

        IEnumerable<IStageSlot<WeissCard>> IStageZone<WeissCard>.Slots =>
            new[] { FrontLeft, FrontCenter, FrontRight, BackLeft, BackRight };

        public StageZone(Player owner) : base("Stage", owner, isPublic: true)
        {
            FrontLeft = new StageSlot("FrontLeft", owner);
            FrontCenter = new StageSlot("FrontCenter", owner);
            FrontRight = new StageSlot("FrontRight", owner);
            BackLeft = new StageSlot("BackLeft", owner);
            BackRight = new StageSlot("BackRight", owner);
        }

        public StageSlot FindSlot(WeissCard character)
        {
            return Slots.FirstOrDefault(s => s.Current == character) as StageSlot;
        }

        public bool IsFrontRow(StageSlot slot)
        {
            return slot == FrontLeft || slot == FrontCenter || slot == FrontRight;
        }

        public StageSlot GetOpposingSlot(StageSlot mySlot, StageZone opponentStage)
        {
            if (mySlot == FrontLeft) return opponentStage.FrontRight;
            if (mySlot == FrontCenter) return opponentStage.FrontCenter;
            if (mySlot == FrontRight) return opponentStage.FrontLeft;
            return null;
        }

        public IEnumerable<StageSlot> Slots =>
            new[] { FrontLeft, FrontCenter, FrontRight, BackLeft, BackRight };
    }

    /// <summary>
    /// Weiss用のステージスロット実装。
    /// </summary>
    public class StageSlot : WeissZone, IStageSlot<WeissCard>
    {
        private WeissCard character;
        public MarkerZone Markers { get; }
        public CardOrientation Orientation { get; private set; } = CardOrientation.Upright;

        public StageSlot(string name, Player owner) : base(name, owner, isPublic: true)
        {
            Markers = new MarkerZone(owner, this);
        }

        public void PlaceCharacter(WeissCard card)
        {
            if (character != null)
                throw new InvalidOperationException($"{Name} already occupied");
            character = card;
        }

        public WeissCard RemoveCharacter()
        {
            var c = character;
            character = null;
            return c;
        }

        public WeissCard Current => character;

        public void SetOrientation(CardOrientation orientation) => Orientation = orientation;
    }

    /// <summary>
    /// ステージスロットのマーカー置場。
    /// </summary>
    public class MarkerZone : WeissZone
    {
        private readonly List<WeissCard> markers = new();
        public StageSlot ParentSlot { get; }

        public MarkerZone(Player owner, StageSlot parent)
            : base($"{parent.Name}_Markers", owner, isPublic: true)
        {
            ParentSlot = parent;
        }

        public void AddMarker(WeissCard card) => markers.Add(card);
        public void RemoveMarker(WeissCard card) => markers.Remove(card);
        public IEnumerable<WeissCard> Markers => markers;
    }
}
