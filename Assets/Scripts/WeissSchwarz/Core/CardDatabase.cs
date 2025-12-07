using System.Collections.Generic;
using System.Linq;

namespace GameCore.Database
{
    /// <summary>
    /// カードデータを管理し、検索機能を提供する汎用データベースクラス。
    /// シングルトンとして実装することを想定しています。
    /// </summary>
    /// <typeparam name="TCardData">カードデータの型</typeparam>
    /// <typeparam name="TQuery">カード検索クエリの型</typeparam>
    public abstract class CardDatabase<TCardData, TQuery>
        where TQuery : CardQuery<TCardData>
    {
        protected List<TCardData> allCards = new List<TCardData>();

        /// <summary>
        /// データベースを初期化し、すべてのカードデータをロードします。
        /// </summary>
        public abstract void LoadDatabase();

        /// <summary>
        /// すべてのカードデータを取得します。
        /// </summary>
        public IEnumerable<TCardData> GetAllCards() => allCards;

        /// <summary>
        /// 指定されたクエリを使用してカードを検索します。
        /// </summary>
        /// <param name="query">検索条件</param>
        /// <returns>検索結果のカードリスト</returns>
        public IEnumerable<TCardData> Search(TQuery query)
        {
            return query.Apply(allCards);
        }
    }
}