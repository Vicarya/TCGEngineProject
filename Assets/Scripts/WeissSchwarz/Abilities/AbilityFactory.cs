using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;
using System.Collections; // For IEnumerable

namespace TCG.Weiss
{
    public static class AbilityFactory
    {
        // Regex for Triggers: 【自】 (Auto), 【起】 (Activated), 【永】 (Continuous)
        private static readonly Regex AutoAbilityRegex = new Regex(@"^【自】");
        private static readonly Regex ActivatedAbilityRegex = new Regex(@"^【起】");
        private static readonly Regex ContinuousAbilityRegex = new Regex(@"^【永】");

        // Regex to find costs enclosed in full-width brackets, e.g., ［(1)］ or ［手札のキャラを１枚控え室に置く］
        private static readonly Regex CostRegex = new Regex(@"［(.+?)］");

        public static List<AbilityBase> CreateAbilitiesForCard(WeissCard sourceCard)
        {
            var abilities = new List<AbilityBase>();

            if (!sourceCard.Data.Metadata.TryGetValue("abilities", out object abilitiesObject))
            {
                return abilities; // No abilities found
            }

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
            if (string.IsNullOrWhiteSpace(abilityText)) return;

            var ability = new WeissAbility(sourceCard);
            string remainingText = abilityText;

            // 1. Parse Ability Type (Trigger)
            var autoMatch = AutoAbilityRegex.Match(remainingText);
            if (autoMatch.Success)
            {
                ability.AbilityType = AbilityType.Auto;
                remainingText = remainingText.Substring(autoMatch.Length).Trim();
            }
            else
            {
                var actMatch = ActivatedAbilityRegex.Match(remainingText);
                if (actMatch.Success)
                {
                    ability.AbilityType = AbilityType.Activated;
                    remainingText = remainingText.Substring(actMatch.Length).Trim();
                }
                else
                {
                    var contMatch = ContinuousAbilityRegex.Match(remainingText);
                    if (contMatch.Success)
                    {
                        ability.AbilityType = AbilityType.Continuous;
                        remainingText = remainingText.Substring(contMatch.Length).Trim();
                    }
                    else
                    {
                        // If no trigger is found, it might be a non-standard ability.
                        // For now, we can skip or log it.
                        return;
                    }
                }
            }

            // 2. Parse Cost
            var costMatch = CostRegex.Match(remainingText);
            if (costMatch.Success)
            {
                string costString = costMatch.Groups[1].Value;
                List<ICost> costs = CostFactory.ParseCosts(costString);
                if (costs != null && costs.Count > 0)
                {
                    ability.Costs.AddRange(costs);
                }
                // Remove cost part from the text
                remainingText = remainingText.Remove(costMatch.Index, costMatch.Length).Trim();
            }

            // 3. The rest is the description, parse it for effects
            ability.Description = remainingText;
            List<IEffect> effects = Effects.EffectFactory.ParseEffects(ability.Description);
            if (effects != null && effects.Count > 0)
            {
                ability.Effects.AddRange(effects);
            }

            abilities.Add(ability);
        }
    }
}