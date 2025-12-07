using System;

namespace TCG.Core {
    /// <summary>
    /// カードの能力がいつ発動できるか（アクティベーションタイミング）を定義するクラス。
    /// 特定のフェーズ、特定のイベント、あるいはカスタムロジックに基づいて発動タイミングを指定できます。
    /// </summary>
    public class ActivationWindow {
        /// <summary>
        /// この能力が発動可能なフェーズのIDの配列。
        /// </summary>
        public string[] AllowedPhaseIds = new string[0];

        /// <summary>
        /// この能力が発動可能なイベントの種類の名前の配列。
        /// </summary>
        public string[] AllowedEventTypes = new string[0];

        /// <summary>
        /// 複雑な発動条件を定義するためのカスタム判定デリゲート。
        /// これが設定されている場合、他の条件よりも優先して評価されます。
        /// </summary>
        public Func<GameState, GameEvent, bool> CustomPredicate;

        /// <summary>
        /// 現在のゲーム状態やイベントが、この能力の発動タイミングに合致するかどうかを判定します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        /// <param name="ev">現在発生したゲームイベント（任意）。</param>
        /// <returns>発動タイミングに合致すればtrue、そうでなければfalse。</returns>
        public bool IsActive(GameState state, GameEvent ev = null) {
            // カスタム判定が設定されていれば、それを最優先で評価する
            if (CustomPredicate != null && CustomPredicate(state, ev)) return true;

            // イベントが指定されており、そのイベントタイプが許可リストに含まれていればアクティブ
            if (ev != null && Array.Exists(AllowedEventTypes, s => s == ev.Type.Name)) return true;

            // ゲーム状態が指定されており、現在のフェーズIDが許可リストに含まれていればアクティブ
            if (state!=null && Array.Exists(AllowedPhaseIds, s => s == state.CurrentPhaseId)) return true;
            
            // どの条件にも合致しない場合は非アクティブ
            return false;
        }
    }
}
