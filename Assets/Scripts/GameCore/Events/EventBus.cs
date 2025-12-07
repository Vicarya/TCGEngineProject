using System;
using System.Collections.Generic;

namespace TCG.Core {
    /// <summary>
    /// ゲーム内で発生した単一のイベントを表現するクラス。
    /// イベントの種類（Type）と、イベントに関連するデータ（Data）を保持します。
    /// </summary>
    public class GameEvent
    {
        /// <summary>
        /// イベントの種類を取得します。
        /// </summary>
        public GameEventType Type { get; }

        /// <summary>
        /// イベントに関連する付随データを取得します。
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// GameEventクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="type">イベントの種類。</param>
        /// <param name="data">イベントに関連するデータ（任意）。</param>
        public GameEvent(GameEventType type, object data = null)
        {
            Type = type;
            Data = data;
        }
    }

    /// <summary>
    /// ゲーム内イベントの発行（Raise）と購読（Subscribe）を管理する中央ハブ（Pub/Subパターン）。
    /// これにより、各コンポーネントは互いを直接知ることなく、疎結合に連携できます。
    /// </summary>
    public class EventBus
    {
        /// <summary>
        /// 特定のイベントタイプに紐づくハンドラを管理します。
        /// </summary>
        private readonly Dictionary<int, List<Action<GameEvent>>> _handlers = new();

        /// <summary>
        /// 全てのイベントを購読するハンドラを管理します。
        /// </summary>
        private readonly List<Action<GameEvent>> _allHandlers = new();

        /// <summary>
        /// 特定の種類のイベントが発生したときに呼び出されるハンドラを登録します。
        /// </summary>
        /// <param name="type">購読するイベントの種類。</param>
        /// <param name="handler">イベント発生時に実行する処理。</param>
        public void Subscribe(GameEventType type, Action<GameEvent> handler)
        {
            if (!_handlers.ContainsKey(type.Index))
            {
                _handlers[type.Index] = new List<Action<GameEvent>>();
            }
            _handlers[type.Index].Add(handler);
        }

        /// <summary>
        /// 登録済みのイベントハンドラを解除します。
        /// </summary>
        /// <param name="type">購読を解除するイベントの種類。</param>
        /// <param name="handler">解除するハンドラ。</param>
        public void Unsubscribe(GameEventType type, Action<GameEvent> handler)
        {
            if (_handlers.TryGetValue(type.Index, out var list))
            {
                list.Remove(handler);
            }
        }

        /// <summary>
        /// 種類を問わず、全てのイベントを購読するハンドラを登録します。
        /// </summary>
        /// <param name="handler">イベント発生時に実行する処理。</param>
        public void SubscribeToAll(Action<GameEvent> handler)
        {
            _allHandlers.Add(handler);
        }

        /// <summary>
        /// 全イベントを購読していたハンドラの登録を解除します。
        /// </summary>
        /// <param name="handler">解除するハンドラ。</param>
        public void UnsubscribeFromAll(Action<GameEvent> handler)
        {
            _allHandlers.Remove(handler);
        }

        /// <summary>
        /// イベントを発行し、登録されている全てのハンドラに通知します。
        /// </summary>
        /// <param name="evt">発行するイベント。</param>
        public void Raise(GameEvent evt)
        {
            // 特定のイベントタイプのハンドラを呼び出し
            if (_handlers.TryGetValue(evt.Type.Index, out var list))
            {
                // ループ中にハンドラがUnsubscribeされる可能性を考慮し、コレクションのコピーに対してループを実行する
                foreach (var h in list.ToArray())
                {
                    h(evt);
                }
            }
            // 全てのイベントを購読しているハンドラを呼び出し
            // こちらも同様に、安全のためコピーに対してループを実行する
            foreach (var h in _allHandlers.ToArray())
            {
                h(evt);
            }
        }
    }
}
