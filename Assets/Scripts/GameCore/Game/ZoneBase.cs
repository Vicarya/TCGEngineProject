using System.Collections.Generic;
using System;
using System.Linq;


namespace TCG.Core
{
    /// <summary>
    /// ゾーンの汎用的な基底クラス。
    /// TCard 型のカードコレクションを内部に持ち、基本的な追加・削除操作を提供します。
    /// 各ゲーム固有のゾーンは、このクラスを継承して特殊な振る舞いを実装します。
    /// </summary>
    public abstract class ZoneBase<TCard> : IZone<TCard> where TCard : Card
    {
        public string Name { get; protected set; }
        public Player Owner { get; protected set; }

        protected ZoneBase() { }

        protected ZoneBase(string name, Player owner)
        {
            Name = name;
            Owner = owner;
        }

        /// <summary>
        /// このゾーンに属するカードのリスト。
        /// protected にすることで、派生クラスは直接このリストを操作できます。
        /// </summary>
        protected List<TCard> cards = new List<TCard>();

        public virtual void AddCard(TCard card)
        {
            cards.Add(card);
        }

        public virtual void RemoveCard(TCard card)
        {
            cards.Remove(card);
        }

        // 非ジェネリック版 IZone の明示的な実装
        void IZone.AddCard(Card card)
        {
            if (card is TCard tCard) AddCard(tCard);
            else throw new ArgumentException("Invalid card type for this zone");
        }

        void IZone.RemoveCard(Card card)
        {
            if (card is TCard tCard) RemoveCard(tCard);
            else throw new ArgumentException("Invalid card type for this zone");
        }

        public IReadOnlyList<TCard> Cards => cards;
    }
    
    /// <summary>
    /// 山札（Deck）ゾーンの汎用的な基底クラス。
    /// 通常のゾーン操作に加え、シャッフル、ドロー、ピーク（カードを覗き見る）といった
    /// 山札特有の機能を提供します。
    /// </summary>
    public abstract class DeckZoneBase<TCard> : ZoneBase<TCard> where TCard : Card
    {
        public DeckZoneBase(string name, Player owner) : base(name, owner) {}

        public virtual TCard PeekTop()
        {
            return cards.Count > 0 ? cards[0] : null;
        }

        public virtual IReadOnlyList<TCard> Peek(int count) {
            return cards.Take(count).ToList().AsReadOnly();
        }

        public virtual TCard DrawTop()
        {
            if (cards.Count == 0) return null;
            var c = cards[0];
            RemoveCard(c);
            return c;
        }

        public virtual void Shuffle()
        {
            var rand = new System.Random();
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }

}
