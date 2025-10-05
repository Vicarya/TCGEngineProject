using System;
using System.Collections.Generic;
using TCG.Core;
using TCG.Core.Costs;
using TCG.Weiss.Triggers;
using WeissSchwarz.Abilities; // For ConcentrateEffect

namespace TCG.Weiss 
{
    public static class AbilityFactory 
    {
        public static AbilityBase CreateFromDefinition(WeissCard self, AbilityDefinition def) 
        {
            var ability = new AbilityBase(self);

            // 1. コストを解決
            if (def.Cost != null)
            {
                if (def.Cost.Stock > 0)
                {
                    ability.Costs.Add(new StockCost<WeissCard>(def.Cost.Stock));
                }
                if (def.Cost.Discard > 0)
                {
                    ability.Costs.Add(new DiscardCost<WeissCard>(def.Cost.Discard));
                }
                if (def.Cost.RestSelf)
                {
                    // RestSelfCostはまだWeissSchwarz名前空間にあると仮定
                    ability.Costs.Add(new TCG.WeissSchwarz.Costs.RestSelfCost<WeissCard>(self));
                }
            }

            // 2. 効果を解決
            if (def.Effects != null)
            {
                foreach (var effectDef in def.Effects)
                {
                    switch (effectDef.Kind)
                    {
                        case EffectKind.Custom:
                            if (effectDef.CustomId == "Concentrate")
                            {
                                ability.Effects.Add(new ConcentrateEffect());
                            }
                            // TODO: 他のカスタムエフェクトもここに追加
                            break;
                        
                        // TODO: 他のEffectKind（Draw, Damageなど）の変換もここに追加
                    }
                }
            }

            // 3. トリガー条件を解決
            if (def.Condition != null)
            {                if (def.Condition.Event == "OnPhaseStart")
                {
                    ability.TriggerConditions.Add(new OnPhaseStartTrigger(def.Condition.Phase));
                }
                // TODO: 他のイベント（OnPlay, OnAttackなど）のトリガーもここに追加
            }

            return ability;
        }
    }
}
