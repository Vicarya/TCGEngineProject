using GameCore.Database;
using TCG.Weiss;
using System.Linq;

namespace WeissSchwarz.Database
{
    /// <summary>
    /// ヴァイスシュヴァルツカードの検索クエリクラス。
    /// </summary>
    public class WeissCardQuery : CardQuery<WeissCardData>
    {
        public WeissCardQuery FilterByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Filters.Add(card => card.name.Contains(name));
            }
            return this;
        }

        public WeissCardQuery FilterByColor(string color)
        {
            if (!string.IsNullOrEmpty(color))
            {
                Filters.Add(card => card.Color == color);
            }
            return this;
        }

        public WeissCardQuery FilterByLevel(int level)
        {
            Filters.Add(card => card.Level == level); // Levelはint型なので変更なし
            return this;
        }

        public WeissCardQuery FilterByTrait(string trait)
        {
            if (!string.IsNullOrEmpty(trait))
            {
                Filters.Add(card => card.Traits.Contains(trait)); // TraitsはList<string>なので変更なし
            }
            return this;
        }

        // 他にも必要なフィルタ条件（コスト、パワー、トリガーなど）を同様に追加

        public WeissCardQuery SortByLevel(bool ascending = true)
        {
            if (ascending)
                Sorter = cards => cards.OrderBy(c => c.Level);
            else
                Sorter = cards => cards.OrderByDescending(c => c.Level);
            return this;
        }

        public WeissCardQuery SortByName(bool ascending = true)
        {
            Sorter = cards => ascending ? cards.OrderBy(c => c.name) : cards.OrderByDescending(c => c.name);
            return this;
        }
    }
}