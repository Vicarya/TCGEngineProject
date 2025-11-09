using System;
using System.Collections.Generic;
using System.Linq;
using CostsCollection = TCG.Core.Costs;

namespace TCG.Core
{
    /// <summary>
    /// 全てのカード能力の基底クラス
    /// </summary>
    public class AbilityBase
    {
        /// <summary>この能力を持つカード</summary>
        public Card SourceCard { get; internal set; }

        /// <summary>発動可能なタイミング（例: AttackStep, MainPhase）</summary>
        public ActivationWindow Activation { get; }

        /// <summary>この能力を発動するためのコスト（無い場合は空）</summary>
        public CostsCollection Costs { get; private set; } = new CostsCollection();

        public List<ITriggerCondition> TriggerConditions { get; } = new();
        public List<IEffect> Effects { get; } = new();

        public AbilityBase(Card source)
        {
            SourceCard = source;
        }

        /// <summary>
        /// イベントを受け取ったときに発動可能かどうか判定
        /// （トリガー条件 + コストチェック）
        /// </summary>
        public bool CanActivate(GameEvent e, GameState state, Player controller)
        {
            if (!CanTrigger(e, state)) return false;
            return Costs.CanPay(state, controller, SourceCard); // Pass SourceCard
        }

        /// <summary>
        /// 発動条件（トリガー）が合っているかどうか
        /// </summary>
        public virtual bool CanTrigger(GameEvent e, GameState state)
        {
            if (TriggerConditions.Count == 0) return true; // 条件がなければ常に発動可能
            return TriggerConditions.All(c => c.IsSatisfied(e, state, SourceCard));
        }

        /// <summary>
        /// 実際に能力を解決する処理（効果本体）
        /// </summary>
        public virtual void Resolve(GameEvent e, GameState state)
        {
            foreach (var effect in Effects)
            {
                effect.Resolve(e, state, SourceCard);
            }
        }

        /// <summary>
        /// 外部から呼ぶ発動処理（チェック済みを前提とする）
        /// </summary>
        public void Activate(GameEvent e, GameState state, Player controller)
        {
            if (!CanActivate(e, state, controller))
                throw new InvalidOperationException("コスト不足または条件未達成で発動不可");

            // コストを支払う
            if (!Costs.IsEmpty)
                Costs.Pay(state, controller, SourceCard); // Pass SourceCard

            // 効果を解決する
            Resolve(e, state);
        }
    }
}
