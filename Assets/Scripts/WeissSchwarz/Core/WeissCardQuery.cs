using TCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツカードの検索クエリを構築するための具象クラス。
    /// </summary>
    public class WeissCardQuery : CardQuery<WeissCardData>
    {
        private static readonly Dictionary<string, string[]> ColorAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Yellow", new[] { "Yellow", "黄", "黄色" } },
            { "Green", new[] { "Green", "緑", "緑色" } },
            { "Red", new[] { "Red", "赤", "赤色" } },
            { "Blue", new[] { "Blue", "青", "青色" } },
        };

        private static readonly Dictionary<string, string[]> CardTypeAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Character", new[] { "Character", "Character Card", "キャラ", "キャラクター" } },
            { "Event", new[] { "Event", "イベント" } },
            { "Climax", new[] { "Climax", "クライマックス" } },
        };

        /// <summary>
        /// 指定した文字列がカード名に含まれているかでフィルタリングします。
        /// </summary>
        public WeissCardQuery HasName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Filters.Add(c => !string.IsNullOrEmpty(c.Name) && c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
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
                string[] acceptedValues = ResolveAliases(ColorAliases, color);
                Filters.Add(c => MatchesAnyAlias(c.Color, acceptedValues));
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
                string[] acceptedValues = ResolveAliases(CardTypeAliases, cardType);
                Filters.Add(c => MatchesAnyAlias(c.CardType, acceptedValues));
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
                Filters.Add(c => !string.IsNullOrEmpty(c.Side) && c.Side.Equals(side, StringComparison.OrdinalIgnoreCase));
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
                Filters.Add(c => !string.IsNullOrEmpty(c.TriggerIcon) && c.TriggerIcon.Equals(trigger, StringComparison.OrdinalIgnoreCase));
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
                Filters.Add(c => !string.IsNullOrEmpty(c.CardCode) && c.CardCode.Contains(cardCode, StringComparison.OrdinalIgnoreCase));
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
                Filters.Add(c =>
                    (!string.IsNullOrEmpty(c.WorkId) && c.WorkId.Equals(workId, StringComparison.OrdinalIgnoreCase)) ||
                    ExtractWorkIdFromCardCode(c.CardCode).Equals(workId, StringComparison.OrdinalIgnoreCase));
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
                Filters.Add(c => !string.IsNullOrEmpty(c.Rarity) && c.Rarity.Equals(rarity, StringComparison.OrdinalIgnoreCase));
            }
            return this;
        }

        private static string[] ResolveAliases(Dictionary<string, string[]> aliases, string value)
        {
            return aliases.TryGetValue(value, out string[] resolvedValues)
                ? resolvedValues
                : new[] { value };
        }

        private static bool MatchesAnyAlias(string actualValue, IEnumerable<string> acceptedValues)
        {
            if (string.IsNullOrEmpty(actualValue))
            {
                return false;
            }

            return acceptedValues.Any(value => actualValue.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        private static string ExtractWorkIdFromCardCode(string cardCode)
        {
            if (string.IsNullOrEmpty(cardCode))
            {
                return string.Empty;
            }

            int separatorIndex = cardCode.IndexOf('/');
            return separatorIndex > 0 ? cardCode.Substring(0, separatorIndex) : cardCode;
        }
    }
}
