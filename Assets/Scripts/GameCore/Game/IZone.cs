using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// あらゆるゾーンが実装すべき非ジェネリックなインターフェース
    /// </summary>
    public interface IZone
    {
        void AddCard(Card card);
        void RemoveCard(Card card);
    }

    /// <summary>
    /// 型指定されたゾーンが実装すべき基本的な操作を定義するインターフェース
    /// </summary>
    /// <typeparam name="TCard">ゾーンが扱うカードの型</typeparam>
    public interface IZone<TCard> : IZone where TCard : Card
    {
        IReadOnlyList<TCard> Cards { get; }

        new void AddCard(TCard card);
        new void RemoveCard(TCard card);
    }
}
