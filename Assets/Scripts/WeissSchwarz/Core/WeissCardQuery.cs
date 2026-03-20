using GameCore.Database;
using System;
using System.Linq;
using TCG.Weiss;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツカードの検索クエリを構築するための具象クラス。
    /// </summary>
    public class WeissCardQuery : CardQuery<WeissCardData>
    {
        /// <summary>
        /// 指定した文字列がカード名に含まれているかでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Filters.Add(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定したレベルでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasLevel(int? level)
        {
            if (level.HasValue)
            {
                Filters.Add(c => c.Level == level.Value);
            }
            return this;
        }

        /// <summary>
        /// 指定したコストでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasCost(int? cost)
        {
            if (cost.HasValue)
            {
                Filters.Add(c => c.Cost == cost.Value);
            }
            return this;
        }

        /// <summary>
        /// 指定した色でフィルタリングします。
        /// </summary>
        public WeissCardQuery HasColor(string color)
        {
            if (!string.IsNullOrEmpty(color))
            {
                Filters.Add(c => c.Color.Equals(color, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定したカードタイプでフィルタリングします。
        /// </summary>
        public WeissCardQuery IsCardType(string cardType)
        {
            if (!string.IsNullOrEmpty(cardType))
            {
                Filters.Add(c => c.CardType.Equals(cardType, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定したサイドでフィルタリングします。
        /// </summary>
        public WeissCardQuery IsSide(string side)
        {
            if (!string.IsNullOrEmpty(side))
            {
                Filters.Add(c => c.Side.Equals(side, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定した特徴を少なくとも1つ持っているかでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasTrait(string trait)
        {
            if (!string.IsNullOrEmpty(trait))
            {
                Filters.Add(c => c.Traits != null && c.Traits.Any(t => t.Contains(trait, StringComparison.OrdinalIgnoreCase)));
            }
            return this;
        }

        /// <summary>
        /// 指定したトリガーアイコンでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasTrigger(string trigger)
        {
            if (!string.IsNullOrEmpty(trigger))
            {
                Filters.Add(c => c.TriggerIcon.Equals(trigger, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定したカード番号でフィルタリングします。
        /// </summary>
        public WeissCardQuery HasCardCode(string cardCode)
        {
            if (!string.IsNullOrEmpty(cardCode))
            {
                Filters.Add(c => c.CardCode.Contains(cardCode, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定した作品IDでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasWorkId(string workId)
        {
            if (!string.IsNullOrEmpty(workId))
            {
                Filters.Add(c => c.WorkId != null && c.WorkId.Equals(workId, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        /// <summary>
        /// 指定したレアリティでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasRarity(string rarity)
        {
            if (!string.IsNullOrEmpty(rarity))
            {
                Filters.Add(c => c.Rarity.Equals(rarity, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }
    }
}
