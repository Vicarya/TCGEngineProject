using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// カードのコレクションを特定の条件でフィルタリングするための契約（インターフェース）を定義します。
    /// これを実装することで、様々なカード検索条件を表現できます。
    /// </summary>
    /// <typeparam name="TCard">フィルタリング対象のカードの型。</typeparam>
    public interface ICardFilter<TCard> where TCard : Card
    {
        /// <summary>
        /// 指定されたカードのコレクションにフィルターを適用します。
        /// </summary>
        /// <param name="source">フィルタリング対象となる元のカードのコレクション。</param>
        /// <returns>フィルター条件に一致したカードのみを含む新しいコレクション。</returns>
        IEnumerable<TCard> Apply(IEnumerable<TCard> source);
    }
}
