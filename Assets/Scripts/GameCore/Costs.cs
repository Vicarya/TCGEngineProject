using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Core
{
    /// <summary>
    /// 複数のコスト（ICost）をまとめて管理するためのクラス。
    /// 能力やイベントの発動に必要なコストを一括して処理します。
    /// </summary>
    public class Costs
    {
        private readonly List<ICost> costList = new();

        /// <summary>
        /// 支払うべきコストが登録されているかどうかを取得します。
        /// </summary>
        public bool IsEmpty => costList.Count == 0;

        /// <summary>
        /// 新しいコストをリストに追加します。
        /// </summary>
        /// <param name="cost">追加するコスト。</param>
        /// <returns>メソッドチェーンのために自身のインスタンスを返します。</returns>
        public Costs Add(ICost cost)
        {
            costList.Add(cost);
            return this;
        }

        /// <summary>
        ///複数のコストをリストにまとめて追加します。
        /// </summary>
        /// <param name="costs">追加するコストのコレクション。</param>
        /// <returns>メソッドチェーンのために自身のインスタンスを返します。</returns>
        public Costs AddRange(IEnumerable<ICost> costs)
        {
            if (costs == null) return this;
            foreach (var c in costs)
            {
                if (c != null) costList.Add(c);
            }
            return this;
        }

        /// <summary>
        /// 登録されている全てのコストが支払い可能かどうかを判定します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        /// <returns>全てのコストが支払い可能であればtrue、そうでなければfalse。</returns>
        public bool CanPay(GameState state, Player player, Card source)
        {
            return costList.All(cost => cost.CanPay(state, player, source));
        }

        /// <summary>
        /// 登録されている全てのコストを支払います。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="player">コストを支払うプレイヤー。</param>
        /// <param name="source">コストの発生源であるカード。</param>
        public void Pay(GameState state, Player player, Card source)
        {
            foreach (var cost in costList)
            {
                cost.Pay(state, player, source);
            }
        }

        /// <summary>
        /// コストリストの列挙子を取得します。
        /// </summary>
        /// <returns>ICostの列挙子。</returns>
        public IEnumerator<ICost> GetEnumerator()
        {
            return costList.GetEnumerator();
        }

        /// <summary>
        /// 登録されているコストの数を取得します。
        /// </summary>
        public int Count => costList.Count;
    }
}
