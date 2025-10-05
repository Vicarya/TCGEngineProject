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
        private Dictionary<int, List<Action<GameEvent>>> handlers = new();

        public void Subscribe(GameEventType type, Action<GameEvent> handler)
        {
            if (!handlers.ContainsKey(type.Index))
                handlers[type.Index] = new List<Action<GameEvent>>();
            handlers[type.Index].Add(handler);
        }

        public void Raise(GameEvent evt)
        {
            if (!handlers.TryGetValue(evt.Type.Index, out var list)) return;
            foreach (var h in list.ToArray()) h(evt);
        }
    }
}