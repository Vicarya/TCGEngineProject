using TCG.Core;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ゲーム内のイベントをリッスンし、UIにログとして表示するMonoBehaviour。
    /// </summary>
    public class GameLogUI : MonoBehaviour
    {
        [Tooltip("イベントログを表示するTextMeshProUGUIコンポーネント")]
        public TextMeshProUGUI logText;

        [Tooltip("表示するログの最大行数")]
        public int maxLogLines = 50;

        private readonly Queue<string> _logMessages = new Queue<string>();

        void Start()
        {
            if (logText == null)
            {
                Debug.LogError("LogText is not assigned in the inspector!", this);
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.Game != null)
            {
                GameManager.Instance.Game.GameState.EventBus.SubscribeToAll(OnGameEvent);
                AddLog("EventLogUI initialized and subscribed to EventBus.");
            }
            else
            {
                Debug.LogError("GameManager or Game instance not found!");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.Game != null)
            {
                GameManager.Instance.Game.GameState.EventBus.UnsubscribeFromAll(OnGameEvent);
            }
        }

        private void OnGameEvent(GameEvent gameEvent)
        {
            string message = $"[{DateTime.Now:HH:mm:ss}] {gameEvent.Type.Name}";
            
            if (gameEvent.Data != null)
            {
                var dataString = gameEvent.Data.ToString()
                    .Replace("{ ", "")
                    .Replace(" }", "")
                    .Replace("=", ": ")
                    .Trim();
                if (!string.IsNullOrEmpty(dataString))
                {
                    message += $" ({dataString})";
                }
            }

            AddLog(message);
        }

        public void AddLog(string message)
        {
            if (_logMessages.Count >= maxLogLines)
            {
                _logMessages.Dequeue();
            }
            _logMessages.Enqueue(message);

            UpdateLogText();
        }

        private void UpdateLogText()
        {
            if(logText != null)
            {
                logText.text = string.Join("\n", _logMessages.Reverse());
            }
        }
    }
}