using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Core
{
    /// <summary>
    /// カード検索クエリの抽象基底クラス。
    /// 流暢なインターフェースを提供します。
    /// </summary>
    /// <typeparam name="TCard">検索対象のカードデータ型</typeparam>
    /// <typeparam name="TQuery">具象クエリクラスの型</typeparam>
    public abstract class CardQuery<TCard, TQuery>
        where TCard : CardData
        where TQuery : CardQuery<TCard, TQuery>
    {
        /// <summary>
        /// フィルタリング条件のリスト。
        /// </summary>
        protected readonly List<Func<TCard, bool>> Filters = new List<Func<TCard, bool>>();

        /// <summary>
        /// ソート条件。
        /// </summary>
        protected Func<IEnumerable<TCard>, IOrderedEnumerable<TCard>> Sorter;

        /// <summary>
        /// フィルタリング条件を追加します。
        /// </summary>
        /// <param name="filter">カードに適用するフィルタリング述語</param>
        /// <returns>クエリインスタンス</returns>
        public TQuery Where(Func<TCard, bool> filter)
        {
            Filters.Add(filter);
            return (TQuery)this;
        }

        /// <summary>
        /// 指定されたキーで昇順にソートします。
        /// </summary>
        /// <param name="keySelector">ソートキーを選択する関数</param>
        /// <returns>クエリインスタンス</returns>
        public TQuery OrderBy<TKey>(Func<TCard, TKey> keySelector)
        {
            Sorter = cards => cards.OrderBy(keySelector);
            return (TQuery)this;
        }

        /// <summary>
        /// 指定されたキーで降順にソートします。
        /// </summary>
        /// <param name="keySelector">ソートキーを選択する関数</param>
        /// <returns>クエリインスタンス</returns>
        public TQuery OrderByDescending<TKey>(Func<TCard, TKey> keySelector)
        {
            Sorter = cards => cards.OrderByDescending(keySelector);
            return (TQuery)this;
        }

        /// <summary>
        /// 指定されたカードリストに現在のクエリ（フィルタとソート）を適用します。
        /// </summary>
        /// <param name="allCards">検索対象となるすべてのカードのリスト</param>
        /// <returns>クエリ適用後のカードのシーケンス</returns>
        public IEnumerable<TCard> Apply(IEnumerable<TCard> allCards)
        {
            IEnumerable<TCard> filtered = allCards;
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