using System;
using System.Collections.Generic;
namespace TCG.Core {
    /// <summary>
    /// ゲームのターンを構成する個々の「フェーズ」（例：ドローフェイズ、アタックフェイズ）を表現する抽象基底クラス。
    /// フェーズは入れ子構造を持つことができます。
    /// </summary>
    public abstract class PhaseBase {
        /// <summary>
        /// フェーズを一意に識別するためのID。
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// フェーズの表示名。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// このフェーズがサブフェーズである場合、その親フェーズを取得します。
        /// </summary>
        public PhaseBase Parent { get; private set; }

        /// <summary>
        /// このフェーズに含まれるサブフェーズのリスト。
        /// </summary>
        private List<PhaseBase> subPhases = new();

        /// <summary>
        /// このフェーズに含まれるサブフェーズの読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<PhaseBase> SubPhases => subPhases.AsReadOnly();

        /// <summary>
        /// PhaseBaseクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="id">フェーズのID。</param>
        /// <param name="name">フェーズの表示名。</param>
        public PhaseBase(string id, string name) { Id = id; Name = name; }

        /// <summary>
        /// このフェーズにサブフェーズを追加します。
        /// </summary>
        /// <param name="p">追加するサブフェーズ。</param>
        public void AddSubPhase(PhaseBase p) { p.Parent = this; subPhases.Add(p); }

        /// <summary>
        /// このフェーズに入ったときに呼び出される処理。
        /// デフォルトではPhaseStartedイベントを発行します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        public virtual void OnEnter(TCG.Core.GameState state) {
            state.EventBus.Raise(new GameEvent(BaseGameEvents.PhaseStarted, this));
        }

        /// <summary>
        /// このフェーズから出るときに呼び出される処理。
        /// デフォルトではPhaseEndedイベントを発行します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        public virtual void OnExit(TCG.Core.GameState state) {
            state.EventBus.Raise(new GameEvent(BaseGameEvents.PhaseEnded, this));
        }

        /// <summary>
        /// このフェーズを実行します。
        /// デフォルトの実装では、OnEnterを呼び出し、全てのサブフェーズを順番に実行し、最後にOnExitを呼び出します。
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        public virtual void Execute(TCG.Core.GameState state) {
            OnEnter(state);
            foreach (var sp in subPhases)
            {
                sp.Execute(state);
            }
            OnExit(state);
        }
    }
}
