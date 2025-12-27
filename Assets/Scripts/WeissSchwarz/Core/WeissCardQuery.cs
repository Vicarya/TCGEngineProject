using GameCore.Database;
using TCG.Weiss;
using System.Linq;

namespace WeissSchwarz.Database
{
    /// <summary>
    /// ヴァイスシュヴァルツカードの検索条件を構築するためのクエリビルダー。
    /// メソッドチェーン（例: new WeissCardQuery().FilterByColor("Red").SortByLevel()）を可能にする
    /// ビルダーパターンで実装されています。
    /// </summary>
    public class WeissCardQuery : CardQuery<WeissCardData>
    {
        /// <summary>
        /// カード名（部分一致）で絞り込みます。
        /// </summary>
        /// <param name="name">検索するカード名の文字列。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery FilterByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                // 基底クラスのFiltersリストに、名前での絞り込み条件（ラムダ式）を追加
                Filters.Add(card => card.name.Contains(name));
            }
            return this;
        }

        /// <summary>
        /// カードの色で絞り込みます。
        /// </summary>
        /// <param name="color">絞り込む色（"Red", "Blue"など）。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery FilterByColor(string color)
        {
            if (!string.IsNullOrEmpty(color))
            {
                Filters.Add(card => card.Color == color);
            }
            return this;
        }

        /// <summary>
        /// カードのレベルで絞り込みます。
        /// </summary>
        /// <param name="level">絞り込むレベル。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery FilterByLevel(int level)
        {
            Filters.Add(card => card.Level == level);
            return this;
        }

        /// <summary>
        /// カードの特徴で絞り込みます。
        /// </summary>
        /// <param name="trait">絞り込む特徴（例：「音楽」）。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery FilterByTrait(string trait)
        {
            if (!string.IsNullOrEmpty(trait))
            {
                Filters.Add(card => card.Traits != null && card.Traits.Contains(trait));
            }
            return this;
        }

        // 他にも必要なフィルタ条件（コスト、パワー、トリガーなど）を同様に追加

        /// <summary>
        /// レベルで並べ替えます。
        /// </summary>
        /// <param name="ascending">昇順の場合はtrue、降順の場合はfalse。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery SortByLevel(bool ascending = true)
        {
            // 基底クラスのSorterに、レベルでの並べ替え処理（ラムダ式）を代入
            if (ascending)
                Sorter = cards => cards.OrderBy(c => c.Level);
            else
                Sorter = cards => cards.OrderByDescending(c => c.Level);
            return this;
        }

        /// <summary>
        /// カード名で並べ替えます。
        /// </summary>
        /// <param name="ascending">昇順の場合はtrue、降順の場合はfalse。</param>
        /// <returns>メソッドチェーンのための自身のインスタンス。</returns>
        public WeissCardQuery SortByName(bool ascending = true)
        {
            Sorter = cards => ascending ? cards.OrderBy(c => c.name) : cards.OrderByDescending(c => c.name);
            return this;
        }
    }
}