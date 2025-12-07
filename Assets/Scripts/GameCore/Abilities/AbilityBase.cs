using System;
using System.Collections.Generic;
using System.Linq;
using CostsCollection = TCG.Core.Costs;

namespace TCG.Core
{
    /// <summary>
    /// 全てのカード能力の基底となる抽象クラス。
    /// カードの能力は、発動タイミング、トリガー条件、コスト、効果の組み合わせで表現されます。
    /// </summary>
    public class AbilityBase
    {
        /// <summary>
        /// この能力を持つカード（能力の発生源）。
        /// </summary>
        public Card SourceCard { get; internal set; }

        /// <summary>
        /// この能力が発動可能なタイミング（例: AttackStep, MainPhase）を取得します。
        /// </summary>
        public ActivationWindow Activation { get; }

        /// <summary>
        /// この能力を発動するためのコストのコレクション。コストがない場合は空です。
        /// </summary>
        public CostsCollection Costs { get; private set; } = new CostsCollection();

        /// <summary>
        /// この能力が自動で発動するためのトリガー条件のリスト。
        /// </summary>
        public List<ITriggerCondition> TriggerConditions { get; } = new();

        /// <summary>
        /// この能力が解決されたときに実行される効果のリスト。
        /// </summary>
        public List<IEffect> Effects { get; } = new();

        /// <summary>
        /// AbilityBaseクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="source">この能力の発生源となるカード。</param>
        public AbilityBase(Card source)
        {
            SourceCard = source;
        }

        /// <summary>
        /// 指定されたゲームイベントをトリガーとして、この能力が発動可能かどうかを判定します。
        /// トリガー条件とコストの両方をチェックします。
        /// </summary>
        /// <param name="e">発生したゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="controller">能力をコントロールするプレイヤー。</param>
        /// <returns>発動可能であればtrue、そうでなければfalse。</returns>
        public bool CanActivate(GameEvent e, GameState state, Player controller)
        {
            // まずトリガー条件を満たすかチェック
            if (!CanTrigger(e, state)) return false;
            // 次にコストが支払えるかチェック
            return Costs.CanPay(state, controller, SourceCard);
        }

        /// <summary>
        /// この能力のトリガー条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="e">発生したゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <returns>全てのトリガー条件が満たされていればtrue、そうでなければfalse。</returns>
        public virtual bool CanTrigger(GameEvent e, GameState state)
        {
            // トリガー条件が何もなければ、常に発動可能とみなす
            if (TriggerConditions.Count == 0) return true;
            // 全てのトリガー条件が満たされているかを確認
            return TriggerConditions.All(c => c.IsSatisfied(e, state, SourceCard));
        }

        /// <summary>
        /// 能力を解決し、登録されている全ての効果を順に実行します。
        /// </summary>
        /// <param name="e">能力の解決をトリガーしたゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        public virtual void Resolve(GameEvent e, GameState state)
        {
            foreach (var effect in Effects)
            {
                effect.Resolve(e, state, SourceCard);
            }
        }

        /// <summary>
        /// 能力の発動を試みます。CanActivateでのチェックが成功していることを前提とします。
        /// コストの支払いと効果の解決を順に行います。
        /// </summary>
        /// <param name="e">能力の発動をトリガーしたゲームイベント。</param>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="controller">能力をコントロールするプレイヤー。</param>
        /// <exception cref="InvalidOperationException">CanActivateがfalseの場合にスローされます。</exception>
        public void Activate(GameEvent e, GameState state, Player controller)
        {
            if (!CanActivate(e, state, controller))
            {
                throw new InvalidOperationException("コスト不足またはトリガー条件未達成のため、能力は発動できません。");
            }

            // コストが設定されていれば支払う
            if (!Costs.IsEmpty)
            {
                Costs.Pay(state, controller, SourceCard);
            }

            // 効果を解決する
            Resolve(e, state);
        }
    }
}
