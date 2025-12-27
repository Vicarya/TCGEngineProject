
using System.Collections.Generic;
using System.Linq;

namespace TCG.Weiss
{
    /// <summary>
    /// 解決を待っている保留中の能力（アビリティ）を管理するキュー。
    /// ヴァイスシュヴァルツでは、特定のタイミングで複数の自動能力が同時に誘発することがある。
    /// このクラスは、それらの能力を適切な順序で一つずつ解決していくために使われる。
    /// </summary>
    public class AbilityQueue
    {
        // NOTE: 同時に誘発した能力は、ターンプレイヤーから順に解決を選択するため、単純なQueue<T>ではなくList<T>で管理している。
        private readonly List<PendingAbility> _queue = new List<PendingAbility>();

        /// <summary>
        /// 新しい保留中の能力をキューに追加します。
        /// </summary>
        /// <param name="ability">追加する保留中の能力。</param>
        public void Add(PendingAbility ability)
        {
            _queue.Add(ability);
        }

        /// <summary>
        /// 指定されたプレイヤーが解決すべき保留中の能力をすべて取得します。
        /// </summary>
        /// <param name="player">対象のプレイヤー。</param>
        /// <returns>プレイヤーが解決すべき保留中の能力のリスト。</returns>
        public List<PendingAbility> GetPendingAbilitiesForPlayer(WeissPlayer player)
        {
            // 複数の能力が同時に保留状態になった場合、どちらのプレイヤーが解決すべきかを判断するために使用される。
            return _queue.Where(pa => pa.ResolvingPlayer == player).ToList();
        }

        /// <summary>
        /// 解決済み、あるいは解決がキャンセルされた能力をキューから削除します。
        /// </summary>
        /// <param name="ability">削除する保留中の能力。</param>
        public void Remove(PendingAbility ability)
        {
            _queue.Remove(ability);
        }

        /// <summary>
        /// キューに解決を待っている能力が残っているかどうかを確認します。
        /// </summary>
        /// <returns>保留中の能力が存在する場合は true、それ以外は false。</returns>
        public bool HasPendingAbilities()
        {
            // ゲームの進行ループは、このメソッドがfalseを返すまで、通常のフェーズ進行を停止し、
            // 能力の解決を優先する必要がある。
            return _queue.Any();
        }
    }
}
