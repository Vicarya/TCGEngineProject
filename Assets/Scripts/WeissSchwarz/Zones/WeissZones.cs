using System;
using TCG.Core;
using System.Linq;
using System.Collections.Generic;

namespace TCG.Weiss 
{
    /// <summary>
    /// ヴァイスシュヴァルツのゾーンの基底クラス。
    /// TCG.Core.ZoneBase を継承し、ヴァイスシュヴァルツのカード(WeissCard)を扱うように特化しています。
    /// IsPublic プロパティを持ち、そのゾーンが公開領域か非公開領域かを定義します。
    /// </summary>
    public abstract class WeissZone : ZoneBase<WeissCard>
    {
        /// <summary>
        /// 公開されている領域かどうか（カードの表が見えるか）
        /// </summary>
        public bool IsPublic { get; }

        protected WeissZone(string name, Player owner, bool isPublic) 
            : base(name, owner)
        {
            IsPublic = isPublic;
        }
    }

    /// <summary>
    /// 山札 (Deck) ゾーン。
    /// TCG.Core.DeckZoneBase を継承し、ヴァイスシュヴァルツのルールに合わせた操作を提供します。
    /// カードはリストの先頭（index 0）が「山札の上」として扱われます。
    /// </summary>
    public class DeckZone : DeckZoneBase<WeissCard>, IDeckZone<WeissCard>
    {
        public DeckZone(Player owner) : base("Deck", owner) { }

        /// <summary>上に積む</summary>
        public void AddTop(WeissCard card) => cards.Insert(0, card); // DeckZoneBase.AddCard adds to the bottom.

        /// <summary>下に積む</summary>
        public void AddBottom(WeissCard card) => base.AddCard(card);

        /// <summary>
        /// X枚めくる (公開する)
        /// 控え室や解決領域などに送る前処理で利用
        /// </summary>
        public IReadOnlyList<WeissCard> Reveal(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (cards.Count < count) throw new InvalidOperationException("Not enough cards in deck");
            var revealed = cards.Take(count).ToList();
            cards.RemoveRange(0, count);
            return revealed;
        }

        /// <summary>
        /// リフレッシュ処理：
        /// 山札が0のとき控え室をシャッフルして新しい山札にする。
        /// その後、ペナルティとして山札の一番上をクロックに置きます。
        /// </summary>
        /// <remarks>このメソッドはルールエンジン(WeissRuleEngine)から呼び出されることを想定しています。</remarks>
        public void RefreshFrom(WaitingRoomZone waitingRoom, ClockZone clock)
        {
            if (cards.Count > 0) return;

            // 全部控え室から移す
            foreach(var card in waitingRoom.Cards) {
                AddCard(card);
            }
            waitingRoom.Clear();

            // シャッフル
            Shuffle();

            // 1枚クロックへ
            var refreshPenalty = DrawTop();
            if (refreshPenalty != null) clock.AddCard(refreshPenalty);
        }
    }

    /// <summary>
    /// 手札 (Hand) ゾーン。非公開領域。
    /// カードの追加・削除以外の特別なロジックは持たないため、ZoneBase の機能をそのまま利用します。
    /// </summary>
    public class HandZone : WeissZone, IHandZone<WeissCard>
    {
        public HandZone(Player owner) : base("Hand", owner, isPublic: false) { }
        // Inherits card management from ZoneBase
    }

    /// <summary>
    /// 控え室 (WaitingRoom) ゾーン。公開領域。
    /// ヴァイスシュヴァルツのルールに従い、カードは「上」に積まれます（後から置かれたカードがリストの先頭に来る）。
    /// </summary>
    public class WaitingRoomZone : WeissZone, IDiscardPile<WeissCard>
    {
        public WaitingRoomZone(Player owner) : base("WaitingRoom", owner, isPublic: true) { }

        /// <summary>
        /// カードを控え室の一番上に置きます。
        /// </summary>
        public override void AddCard(WeissCard card) {
            cards.Insert(0, card); // Add to top
        }

        /// <remarks>TODO: このメソッドはカードを単に消去するのではなく、別のゾーン（山札など）に移動させるべきです。</remarks>
        public void Clear() => cards.Clear(); // TODO: This should probably move cards, not just clear the list.
    }

    /// <summary>
    /// カードの向き（立っている/横に寝かせる/逆さ）
    /// </summary>
    public enum CardOrientation
    {
        Upright,
        Sideways,
        Reversed
    }

    /// <summary>
    /// 舞台。前列3・後列2の合計5スロットを持つ。
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

        /// <summary>
        /// 指定されたキャラがどのスロットにいるか検索します。
        /// </summary>
        public StageSlot FindSlot(WeissCard character)
        {
            return Slots.FirstOrDefault(s => s.Current == character) as StageSlot;
        }

        /// <summary>
        /// 指定されたスロットが前列かどうかを判定します。
        /// </summary>
        public bool IsFrontRow(StageSlot slot)
        {
            return slot == FrontLeft || slot == FrontCenter || slot == FrontRight;
        }

        /// <summary>
        /// 指定されたスロットの正面にいる相手のスロットを取得します。
        /// </summary>
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
    /// 舞台のスロット。キャラ1枚のみ。マーカーゾーン付き。
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
    /// マーカー置場。スロットに紐づく。
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

    /// <summary>
    /// クロック置場。公開領域。カードは上に積む形式。順序変更不可。
    /// 7枚以上になるとレベル置場と控え室への移動処理が必要。
    /// </summary>
    public class ClockZone : WeissZone, IClockZone<WeissCard>
    {
        // Owner is inherited from ZoneBase
        public LevelZone LevelZone { get; } // Should be retrieved via Player
        public WaitingRoomZone WaitingRoom { get; }

        public ClockZone(Player owner, LevelZone levelZone, WaitingRoomZone waitingRoom) 
            : base("Clock", owner, isPublic: true)
        {
            Owner = owner;
            LevelZone = levelZone;
            WaitingRoom = waitingRoom;
        }

        /// <summary>
        /// カードを上に置く。7枚以上になった場合はルール処理。
        /// </summary>
        // AddCard is inherited, but we need special logic for Clock.
        public override void AddCard(WeissCard card)
        {
            cards.Add(card);
            // イベント発行はRuleEngineで行うため、ここではカード追加のみ
        }

        /// <summary>
        /// クロックゾーンから指定されたカードをすべて削除します。
        /// レベルアップ処理でRuleEngineから使用されます。
        /// </summary>
        public void RemoveCards(IEnumerable<WeissCard> cardsToRemove)
        {
            foreach(var card in cardsToRemove.ToList())
            {
                cards.Remove(card);
            }
        }

        /// <summary>
        /// 上から取り除く
        /// </summary>
        public WeissCard RemoveTopCard()
        {
            if (!cards.Any()) throw new InvalidOperationException("ClockZone is empty");
            var top = cards[^1];
            cards.RemoveAt(cards.Count - 1);
            return top;
        }
    }

    /// <summary>
    /// レベル置場。公開領域。順序固定。4枚以上で敗北条件。
    /// </summary>
    public class LevelZone : WeissZone, ILevelZone<WeissCard>
    {
        private readonly List<(WeissCard Card, bool FaceUp)> levelCards = new();
        public LevelZone(Player owner) : base("Level", owner, isPublic: true) { }

        /// <summary>
        /// カードを上に置く
        /// </summary>
        public void AddCard(WeissCard card, bool faceUp = true)
        {
            levelCards.Add((card, faceUp));

            // 4枚以上で敗北判定
            if (levelCards.Count >= 4)
            {
                // Owner.LoseGame(); // This logic should be in the RuleEngine
            }
        }

        public IReadOnlyList<(WeissCard Card, bool FaceUp)> LevelCards => levelCards;
        public int Count => levelCards.Count;
    }

    /// <summary>
    /// ストック (Stock) ゾーン。非公開領域。
    /// 後入れ先出し(LIFO)のルールに従い、カードは「上」に積まれます。
    /// </summary>
    public class StockZone : WeissZone, IStockZone<WeissCard>
    {
        public StockZone(Player owner) : base("Stock", owner, isPublic: false) { }
        // Inherits from ZoneBase, which uses a List.
        // Weiss Stock is LIFO, so we override AddCard to insert at the beginning.
        public override void AddCard(WeissCard card) => cards.Insert(0, card);

        public WeissCard RemoveTopCard()
        {
            if (cards.Count == 0) return null;
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
    }

    /// <summary>
    /// クライマックス (Climax) ゾーン。公開領域。
    /// 原則としてクライマックスカードを1枚しか置けません。
    /// </summary>
    public class ClimaxZone : WeissZone, IClimaxZone<WeissCard>
    {
        public ClimaxZone(Player owner) : base("Climax", owner, isPublic: true) { }

        // Override to add validation
        public override void AddCard(WeissCard card)
        {
            if (((WeissCardData)card.Data).CardType != WeissCardType.Climax.ToString())
                throw new InvalidOperationException("クライマックスゾーンにはクライマックスカードしか置けません。");
            if (cards.Count >= 1)
                throw new InvalidOperationException("ClimaxZone can only hold 1 card");
            cards.Add(card);
        }
        
        /// <summary>
        /// 上から取り除く(実質全て取り除く)
        /// </summary>
        public WeissCard RemoveTopCard()
        {
            if (!cards.Any()) throw new InvalidOperationException("ClimaxZone is empty");
            var c = cards[0];
            cards.Clear();
            return c;
        }
    }

    /// <summary>
    /// 思い出置場。基本は公開だが、効果により裏向きで置かれることがある。
    /// </summary>
    public class MemoryZone : WeissZone
    {
        private readonly List<(WeissCard Card, bool FaceUp)> memoryCards = new();

        public MemoryZone(Player owner) : base("Memory", owner, isPublic: true) { }

        public IReadOnlyList<(WeissCard Card, bool FaceUp)> MemoryCards => memoryCards;
        public int Count => memoryCards.Count;

        /// <summary>
        /// カードを追加
        /// </summary>
        public void AddCard(WeissCard card, bool faceUp = true)
        {
            // 新しいカードは上に置く
            memoryCards.Insert(0, (card, faceUp));
        }

        /// <summary>
        /// 裏向きカードの順序や識別を保持しつつ、取り出す
        /// </summary>
        public (WeissCard Card, bool FaceUp) RemoveSpecificCard(WeissCard card)
        {
            var idx = memoryCards.FindIndex(c => c.Card == card);
            if (idx < 0) throw new InvalidOperationException("Card not found in MemoryZone");

            var tuple = memoryCards[idx];
            memoryCards.RemoveAt(idx);
            return tuple;
        }
    }

    /// <summary>
    /// 解決領域。公開領域。一時的にカードが置かれる。順序固定。
    /// </summary>
    public class ResolutionZone : WeissZone
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
