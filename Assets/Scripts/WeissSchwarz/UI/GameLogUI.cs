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

        /// <summary>
        /// UnityのAwakeイベントで呼び出されます。
        /// このスクリプトインスタンスが有効化されたときに一度だけ実行されます。
        /// ここでは、EventBusへの登録を行います。
        /// </summary>
        void Start()
        {
            if (logText == null)
            {
                Debug.LogError("LogText is not assigned in the inspector!", this);
                return;
            }

            // GameManagerとGameインスタンスが初期化されていることを確認し、イベントバスに購読します。
            // これにより、ゲーム内で発生するすべてのイベントをOnGameEventメソッドで受け取ることができます。
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

        /// <summary>
        /// UnityのOnDestroyイベントで呼び出されます。
        /// このスクリプトが破棄される前に、EventBusからの購読を解除します。
        /// メモリリークを防ぐための重要な処理です。
        /// </summary>
        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.Game != null)
            {
                GameManager.Instance.Game.GameState.EventBus.UnsubscribeFromAll(OnGameEvent);
            }
        }

        /// <summary>
        /// ゲーム内で発生したすべてのイベントを受け取り、ログメッセージを生成します。
        /// </summary>
        /// <param name="gameEvent">発生したゲームイベント。</param>
        private void OnGameEvent(GameEvent gameEvent)
        {
            string message = $"[{DateTime.Now:HH:mm:ss}] {gameEvent.Type.Name}";
            
            // イベントデータが存在する場合、その内容を整形してログメッセージに追加します。
            // ToString()で取得した文字列から不要な文字を除去し、見やすい形式に変換しています。
            if (gameEvent.Data != null)
            {
                var dataString = gameEvent.Data.ToString()
                    .Replace("{ ", "") // JSONライクな文字列から開始括弧を除去
                    .Replace(" }", "") // JSONライクな文字列から終了括弧を除去
                    .Replace("=", ": ") // Key=Value形式をKey: Value形式に変換
                    .Trim(); // 前後の空白を除去
                if (!string.IsNullOrEmpty(dataString))
                {
                    message += $" ({dataString})";
                }
            }

            AddLog(message);
        }

        /// <summary>
        /// 指定されたメッセージをログキューに追加し、表示を更新します。
        /// キューのサイズが最大行数を超えた場合、最も古いメッセージが削除されます。
        /// </summary>
        /// <param name="message">ログに追加するメッセージ。</param>
        public void AddLog(string message)
        {
            if (_logMessages.Count >= maxLogLines)
            {
                _logMessages.Dequeue(); // 最大行数を超えた場合、一番古いログを削除
            }
            _logMessages.Enqueue(message); // 新しいログを追加

            UpdateLogText(); // UIの表示を更新
        }

        /// <summary>
        /// ログキューの内容に基づいて、TextMeshProUGUIの表示を更新します。
        /// 最新のログが一番下（最後に表示）されるように、メッセージの順序を反転させています。
        /// </summary>
        private void UpdateLogText()
        {
            if(logText != null)
            {
                // キューに格納されたログメッセージを改行区切りで結合し、ログ表示テキストを更新します。
                // Reverse() を使用することで、最新のログがリストの最後尾（UI上では一番下）に表示されるようにします。
                logText.text = string.Join("\n", _logMessages.Reverse());
            }
        }
    }
}