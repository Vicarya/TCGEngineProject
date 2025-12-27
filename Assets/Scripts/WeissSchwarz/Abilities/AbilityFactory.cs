using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;
using System.Collections;

namespace TCG.Weiss
{
    /// <summary>
    /// カードデータに含まれる能力テキストを解析し、実行可能な能力オブジェクト（WeissAbility）を生成する静的ファクトリクラス。
    /// このプロジェクトのデータ駆動設計の心臓部であり、例えば「【自】［(1)］あなたのクライマックスが置かれた時、あなたは1枚引く。」のような
    /// テキストを、正規表現を用いて以下の3要素に分解します。
    /// 1. 能力タイプ：【自】(自動)、【起】(起動)、【永】(常時)
    /// 2. コスト：［(1)］
    /// 3. 効果：あなたのクライマックスが置かれた時、あなたは1枚引く。
    ///分解された各要素は、さらにCostFactoryやEffectFactoryに渡されてオブジェクト化されます。
    /// </summary>
    public static class AbilityFactory
    {
        // 能力タイプを識別するための正規表現
        private static readonly Regex AutoAbilityRegex = new Regex(@"^【自】"); // 自動能力
        private static readonly Regex ActivatedAbilityRegex = new Regex(@"^【起】"); // 起動能力
        private static readonly Regex ContinuousAbilityRegex = new Regex(@"^【永】"); // 常時能力

        // コスト部分（全角の角括弧［］で囲まれた部分）を抽出するための正規表現
        private static readonly Regex CostRegex = new Regex(@"［(.+?)］");

        /// <summary>
        /// 指定されたカードデータから、すべての能力オブジェクトを生成します。
        /// </summary>
        /// <param name="sourceCard">能力の発生源となるカード。</param>
        /// <returns>生成された能力オブジェクト（AbilityBase）のリスト。</returns>
        public static List<AbilityBase> CreateAbilitiesForCard(WeissCard sourceCard)
        {
            var abilities = new List<AbilityBase>();

            // カードのメタデータから"abilities"キーで能力テキストのコレクションを取得
            if (!sourceCard.Data.Metadata.TryGetValue("abilities", out object abilitiesObject))
            {
                return abilities; // 能力が見つからなければ空のリストを返す
            }

            if (abilitiesObject is IEnumerable abilityCollection)
            {
                foreach (var abilityObj in abilityCollection)
                {
                    if (abilityObj != null)
                    {
                        // 個々の能力テキストを処理する
                        ProcessAbilityString(abilityObj.ToString(), sourceCard, abilities);
                    }
                }
            }

            return abilities;
        }

        /// <summary>
        /// 単一の能力テキスト文字列を解析し、WeissAbilityオブジェクトを構築してリストに追加します。
        /// </summary>
        /// <param name="abilityText">解析対象の能力テキスト。</param>
        /// <param name="sourceCard">能力の発生源カード。</param>
        /// <param name="abilities">構築した能力オブジェクトを追加する先のリスト。</param>
        private static void ProcessAbilityString(string abilityText, WeissCard sourceCard, List<AbilityBase> abilities)
        {
            if (string.IsNullOrWhiteSpace(abilityText)) return;

            var ability = new WeissAbility(sourceCard);
            string remainingText = abilityText;

            // --- 手順1: 能力タイプの解析 ---
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
                        // 能力タイプが見つからない場合は、標準外の能力として一旦処理をスキップ
                        return;
                    }
                }
            }

            // --- 手順2: コストの解析 ---
            var costMatch = CostRegex.Match(remainingText);
            if (costMatch.Success)
            {
                // ［］で囲まれたコスト文字列を抽出
                string costString = costMatch.Groups[1].Value;
                // CostFactoryに処理を委譲し、ICostオブジェクトのリストを取得
                List<ICost> costs = CostFactory.ParseCosts(costString);
                if (costs != null && costs.Count > 0)
                {
                    ability.Costs.AddRange(costs);
                }
                // 解析済みのコスト部分をテキストから除去
                remainingText = remainingText.Remove(costMatch.Index, costMatch.Length).Trim();
            }

            // --- 手順3: 効果の解析 ---
            // 残ったテキストが効果記述部分となる
            ability.Description = remainingText;
            // EffectFactoryに処理を委譲し、IEffectオブジェクトのリストを取得
            List<IEffect> effects = Effects.EffectFactory.ParseEffects(ability.Description);
            if (effects != null && effects.Count > 0)
            {
                ability.Effects.AddRange(effects);
            }

            abilities.Add(ability);
        }
    }
}