using System;
using System.Collections.Generic;

namespace TCG.Core {
    public class GameEvent
    {
        public GameEventType Type { get; }
        public object Data { get; }

        public GameEvent(GameEventType type, object data = null)
        {
            Type = type;
            Data = data;
        }
    }

    public class EventBus
    {
        private readonly Dictionary<int, List<Action<GameEvent>>> _handlers = new();
        private readonly List<Action<GameEvent>> _allHandlers = new();

        public void Subscribe(GameEventType type, Action<GameEvent> handler)
        {
            if (!_handlers.ContainsKey(type.Index))
            {
                _handlers[type.Index] = new List<Action<GameEvent>>();
            }
            _handlers[type.Index].Add(handler);
        }

        public void Unsubscribe(GameEventType type, Action<GameEvent> handler)
        {
            if (_handlers.TryGetValue(type.Index, out var list))
            {
                list.Remove(handler);
            }
        }

        public void SubscribeToAll(Action<GameEvent> handler)
        {
            _allHandlers.Add(handler);
        }

        public void UnsubscribeFromAll(Action<GameEvent> handler)
        {
            _allHandlers.Remove(handler);
        }

        public void Raise(GameEvent evt)
        {
            if (_handlers.TryGetValue(evt.Type.Index, out var list))
            {
                foreach (var h in list.ToArray())
                {
                    h(evt);
                }
            }
            foreach (var h in _allHandlers.ToArray())
            {
                h(evt);
            }
        }
    }
}
