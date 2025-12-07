using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// カードが存在する場所（ゾーン）を抽象化する、非ジェネリックな基本インターフェース。
    /// これにより、カードの型を意識せずにゾーン間の基本的なカード移動を扱うことができます。
    /// </summary>
    public interface IZone
    {
        /// <summary>
        /// ゾーンにカードを追加します（非ジェネリック版）。
        /// </summary>
        /// <param name="card">追加するカード。</param>
        void AddCard(Card card);

        /// <summary>
        /// ゾーンからカードを削除します（非ジェネリック版）。
        /// </summary>
        /// <param name="card">削除するカード。</param>
        void RemoveCard(Card card);
    }

    /// <summary>
    /// 特定のカード型に特化した、型安全なゾーンが実装すべき操作を定義するインターフェース。
    /// </summary>
    /// <typeparam name="TCard">ゾーンが扱うカードの型。</typeparam>
    public interface IZone<TCard> : IZone where TCard : Card
    {
        /// <summary>
        /// ゾーンに存在するカードの読み取り専用リストを取得します。
        /// </summary>
        IReadOnlyList<TCard> Cards { get; }

        /// <summary>
        /// ゾーンにカードを追加します（型安全なジェネリック版）。
        /// </summary>
        /// <param name="card">追加するカード。</param>
        void AddCard(TCard card);

        /// <summary>
        /// ゾーンからカードを削除します（型安全なジェネリック版）。
        /// </summary>
        /// <param name="card">削除するカード。</param>
        void RemoveCard(TCard card);
    }
}
