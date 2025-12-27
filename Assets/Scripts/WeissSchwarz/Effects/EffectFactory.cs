using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;

namespace TCG.Weiss.Effects
{
    /// <summary>
    /// 能力の効果記述文字列を解析し、具体的な効果オブジェクト（IEffectインターフェースの実装）の
    /// リストに変換する静的ファクトリクラス。
    /// </summary>
    public static class EffectFactory
    {
        // 正規表現と「IEffectオブジェクトを生成する関数」をマッピングするディクショナリ。
        // 新しい種類の効果を追加するには、このディクショナリに新しいエントリーを追加するだけでよく、拡張性が高い。
        private static readonly Dictionary<Regex, System.Func<Match, IEffect>> EffectParsers = new()
        {
            // 例：「あなたは自分の山札を上から4枚見て、山札の上か下に置く」という集中などの効果テキストにマッチ
            { new Regex(@"あなたは自分の山札を上から(\d+)枚見て、山札の上か下に置く"), match => new LookTopAndPlaceEffect(int.Parse(match.Groups[1].Value)) },
            
            // 例：「あなたは自分の控え室のキャラを1枚選び、手札に戻す」という回収効果のテキストにマッチ
            { new Regex(@"あなたは自分の控え室のキャラを(\d+)枚選び、手札に戻す"), match => new ReturnFromWaitingRoomEffect(int.Parse(match.Groups[1].Value)) },
            
            // TODO: 他の種類の効果（例：パワーを上げる、ダメージを与える等）に対応するパーサーをここに追加する
        };

        /// <summary>
        /// 指定された効果記述文字列を解析し、対応するIEffectオブジェクトのリストを返します。
        /// </summary>
        /// <param name="description">解析対象の効果記述文字列。</param>
        /// <returns>解析されたIEffectオブジェクトのリスト。</returns>
        public static List<IEffect> ParseEffects(string description)
        {
            var effects = new List<IEffect>();
            if (string.IsNullOrWhiteSpace(description)) return effects;

            // 定義されたすべての正規表現パターンを、与えられた効果記述文字列に適用する
            foreach (var pair in EffectParsers)
            {
                var regex = pair.Key;
                // １つの記述に同じ効果が複数回含まれる可能性は稀だが、`Matches`で念のためすべて取得
                foreach (Match match in regex.Matches(description))
                {
                    // マッチした正規表現に対応する関数を実行し、IEffectオブジェクトを生成してリストに追加
                    effects.Add(pair.Value(match));
                }
            }

            // どのパターンにもマッチしなかった場合
            if (effects.Count == 0)
            {
                // 未解析の効果が残っている場合に警告ログを出すことも可能
                // UnityEngine.Debug.LogWarning($"解析可能な効果が見つかりませんでした: '{description}'");
            }

            return effects;
        }
    }
}
