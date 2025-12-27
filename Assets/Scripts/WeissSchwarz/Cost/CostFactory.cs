using System.Collections.Generic;
using System.Text.RegularExpressions;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 能力のコスト記述文字列を解析し、具体的なコストオブジェクト（ICostインターフェースの実装）の
    /// リストに変換する静的ファクトリクラス。
    /// </summary>
    public static class CostFactory
    {
        // 正規表現と「ICostオブジェクトを生成する関数」をマッピングするディクショナリ。
        // 新しい種類のコストを追加するには、このディクショナリに新しいエントリーを追加するだけでよいため、拡張性が高い。
        private static readonly Dictionary<Regex, System.Func<Match, ICost>> CostParsers = new()
        {
            // 例: (1), (2) のようなストックコストにマッチ
            { new Regex(@"\((\d+)\)"), match => new StockCost<WeissCard>(int.Parse(match.Groups[1].Value)) },
            
            // 例: (C1), (C2) のようなクロックコストにマッチ (架空のコスト表記)
            { new Regex(@"\(C(\d+)\)"), match => new ClockCost<WeissCard>(int.Parse(match.Groups[1].Value)) },
            
            // 例: 「手札のキャラを１枚控え室に置く」というテキストにマッチ
            { new Regex(@"手札のキャラを(\d+)枚控え室に置く"), match => new DiscardCost(int.Parse(match.Groups[1].Value), card => (card.Data as WeissCardData)?.CardType == "Character") },
            
            // 例: 「このカードを【レスト】する」というテキストにマッチ
            { new Regex(@"このカードを【レスト】する"), match => new RestCost(isSourceCard: true) }
            
            // TODO: 他の種類のコスト（例：マーカーを消費する、特定の特徴を持つカードを捨てる等）に対応するパーサーをここに追加する
        };

        /// <summary>
        /// 指定されたコスト文字列を解析し、対応するICostオブジェクトのリストを返します。
        /// </summary>
        /// <param name="costString">解析対象のコスト文字列。例：「(1) このカードを【レスト】する」</param>
        /// <returns>解析されたICostオブジェクトのリスト。</returns>
        public static List<ICost> ParseCosts(string costString)
        {
            var costs = new List<ICost>();
            if (string.IsNullOrWhiteSpace(costString)) return costs;

            string remainingCostString = costString;

            // 文字列からマッチするコストパターンがなくなるまで解析を繰り返す
            while (!string.IsNullOrWhiteSpace(remainingCostString))
            {
                bool matchFound = false;
                foreach (var pair in CostParsers)
                {
                    var regex = pair.Key;
                    var match = regex.Match(remainingCostString);
                    if (match.Success)
                    {
                        // マッチした正規表現に対応する関数を実行し、ICostオブジェクトを生成
                        costs.Add(pair.Value(match));
                        
                        // 解析済みの部分を文字列から除去
                        remainingCostString = remainingCostString.Remove(match.Index, match.Length).Trim();
                        matchFound = true;
                        
                        // 文字列の先頭から再度すべてのパターンを試し直すため、ループを最初からやり直す
                        break; 
                    }
                }

                // CostParsersのどのパターンにもマッチしなかった場合、
                // それ以上解析できない部分が残っていると判断し、ループを終了する。
                if (!matchFound)
                {
                    // 未解析のコスト部分が残っている場合に警告ログを出すことも可能
                    // UnityEngine.Debug.LogWarning($"未解析のコスト部分: {remainingCostString}");
                    break;
                }
            }

            return costs;
        }
    }
}
