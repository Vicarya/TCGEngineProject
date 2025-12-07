using System.Collections.Generic;
using System;
using System.Linq;

namespace TCG.Core
{
    /// <summary>
    /// ゾーンの汎用的な抽象基底クラス。
    /// TCard 型のカードコレクションを内部に持ち、基本的な追加・削除操作を提供します。
    /// 各ゲーム固有のゾーン（例：手札、舞台）は、このクラスを継承して特殊な振る舞いを実装します。
    /// </summary>
    public abstract class ZoneBase<TCard> : IZone<TCard> where TCard : Card
    {
        /// <summary>
        /// ゾーンの名前（例：「手札」）。
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// このゾーンを所有するプレイヤー。
        /// </summary>
        public Player Owner { get; protected set; }

        protected ZoneBase() { }

        protected ZoneBase(string name, Player owner)
        {
            Name = name;
            Owner = owner;
        }

        /// <summary>
        /// このゾーンに属するカードのリスト。
        /// `protected` にすることで、派生クラスは直接このリストを操作できます。
        /// </summary>
        protected List<TCard> cards = new List<TCard>();

        /// <summary>
        /// このゾーンにカードを追加します。
        /// </summary>
        /// <param name="card">追加するカード。</param>
        public virtual void AddCard(TCard card)
        {
            cards.Add(card);
        }

        /// <summary>
        /// このゾーンからカードを削除します。
        /// </summary>
        /// <param name="card">削除するカード。</param>
        public virtual void RemoveCard(TCard card)
        {
            cards.Remove(card);
        }

        // 非ジェネリック版 IZone の明示的な実装
        /// <summary>
        /// ゾーンにカードを追加します（非ジェネリックなIZoneインターフェースの実装）。
        /// </summary>
        /// <param name="card">追加するカード。</param>
        /// <exception cref="ArgumentException">このゾーンで許容されない型のカードが渡された場合にスローされます。</exception>
        void IZone.AddCard(Card card)
        {
            if (card is TCard tCard)
            {
                AddCard(tCard);
            }
            else
            {
                throw new ArgumentException("このゾーンには無効なカードタイプです。");
            }
        }

        /// <summary>
        /// ゾーンからカードを削除します（非ジェネリックなIZoneインターフェースの実装）。
        /// </summary>
        /// <param name="card">削除するカード。</param>
        /// <exception cref="ArgumentException">このゾーンで許容されない型のカードが渡された場合にスローされます。</exception>
        void IZone.RemoveCard(Card card)
        {
            if (card is TCard tCard)
            {
                RemoveCard(tCard);
            }
            else
            {
                throw new ArgumentException("このゾーンには無効なカードタイプです。");
            }
        }

        /// <summary>
        /// このゾーンに含まれるカードの読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<TCard> Cards => cards;
    }
    
    /// <summary>
    /// 山札（Deck）ゾーンの汎用的な抽象基底クラス。
    /// 通常のゾーン操作に加え、シャッフル、ドロー、ピーク（カードを覗き見る）といった
    /// 山札特有の機能を提供します。
    /// </summary>
    public abstract class DeckZoneBase<TCard> : ZoneBase<TCard> where TCard : Card
    {
        public DeckZoneBase(string name, Player owner) : base(name, owner) {}

        /// <summary>
        /// 山札の一番上のカードを、移動させずに中身だけ見ます。
        /// </summary>
        /// <returns>山札の一番上のカード。山札が空の場合はnull。</returns>
        public virtual TCard PeekTop()
        {
            return cards.Count > 0 ? cards[0] : null;
        }

        /// <summary>
        /// 山札の上から指定された枚数のカードを、移動させずに中身だけ見ます。
        /// </summary>
        /// <param name="count">見る枚数。</param>
        /// <returns>山札の上から指定枚数のカードの読み取り専用リスト。</returns>
        public virtual IReadOnlyList<TCard> Peek(int count) {
            return cards.Take(count).ToList().AsReadOnly();
        }

        /// <summary>
        /// 山札の一番上のカードを1枚引きます（ゾーンから取り除きます）。
        /// </summary>
        /// <returns>引いたカード。山札が空の場合はnull。</returns>
        public virtual TCard DrawTop()
        {
            if (cards.Count == 0) return null;
            var c = cards[0];
            RemoveCard(c);
            return c;
        }

        /// <summary>
        /// 山札をシャッフルします。
        /// </summary>
        public virtual void Shuffle()
        {
            var rand = new System.Random();
            // Fisher-Yatesアルゴリズムでリストをシャッフル
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}
