using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;
using System.Collections; // For IEnumerable

namespace TCG.Weiss
{
    public static class AbilityFactory
    {
        // Regex to find costs enclosed in full-width brackets, e.g., ［(1)］ or ［手札のキャラを１枚控え室に置く］
        private static readonly Regex CostRegex = new Regex(@"［(.+?)］");

        public static List<AbilityBase> CreateAbilitiesForCard(WeissCard sourceCard)
        {
            var abilities = new List<AbilityBase>();

            if (!sourceCard.Data.Metadata.TryGetValue("abilities", out object abilitiesObject))
            {
                return abilities; // No abilities found
            }

            // The object from metadata is likely a List<object> or similar collection.
            if (abilitiesObject is IEnumerable abilityCollection)
            {
                foreach (var abilityObj in abilityCollection)
                {
                    if (abilityObj != null)
                    {
                        ProcessAbilityString(abilityObj.ToString(), sourceCard, abilities);
                    }
                }
            }

            return abilities;
        }

        private static void ProcessAbilityString(string abilityText, WeissCard sourceCard, List<AbilityBase> abilities)
        {
            var match = CostRegex.Match(abilityText);
            if (match.Success)
            {
                string costString = match.Groups[1].Value;
                ICost cost = CostFactory.Parse(costString);

                if (cost != null)
                {
                    // For now, create a generic ability for any text that has a parsable cost.
                    // We can add more logic later to parse triggers and effects.
                    var ability = new AbilityBase(sourceCard);
                    ability.Costs.Add(cost);
                    abilities.Add(ability);
                }
            }
        }
    }
}