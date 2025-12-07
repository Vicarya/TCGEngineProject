using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Database
{
    /// <summary>
    /// カード検索クエリの基底クラス。
    /// </summary>
    /// <typeparam name="TCardData">検索対象のカードデータ型</typeparam>
    public abstract class CardQuery<TCardData>
    {
        /// <summary>
        /// フィルタリング条件のリスト。
        /// 各Funcは、カードデータを受け取り、条件に一致するかどうかを返します。
        /// </summary>
        protected readonly List<Func<TCardData, bool>> Filters = new List<Func<TCardData, bool>>();

        /// <summary>
        /// ソート条件。
        /// カードデータのシーケンスを受け取り、ソートされたシーケンスを返します。
        /// </summary>
        protected Func<IEnumerable<TCardData>, IOrderedEnumerable<TCardData>> Sorter;

        /// <summary>
        /// 指定されたカードリストに現在のクエリ（フィルタとソート）を適用します。
        /// </summary>
        /// <param name="allCards">検索対象となるすべてのカードのリスト</param>
        /// <returns>クエリ適用後のカードリスト</returns>
        public virtual IEnumerable<TCardData> Apply(IEnumerable<TCardData> allCards)
        {
            IEnumerable<TCardData> filtered = allCards;
            foreach (var filter in Filters)
            {
                filtered = filtered.Where(filter);
            }

            if (Sorter != null)
            {
                return Sorter(filtered);
            }

            return filtered;
        }
    }
}