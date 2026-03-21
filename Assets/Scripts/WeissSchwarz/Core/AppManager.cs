using System.Collections;
using System.IO;
using UnityEngine;
using TCG.Weiss.Data;
using UnityEngine.Networking; // UnityWebRequestを使用するために必要

namespace TCG.Weiss
{
    /// <summary>
    /// アプリケーション全体のデータ初期化を管理するシングルトンクラス。
    /// 主に、カードデータ（JSON）の読み込みとSQLiteデータベースへのインポートを担当する。
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        /// <summary>
        /// AppManagerのシングルトンインスタンス。
        /// </summary>
        public static AppManager Instance { get; private set; }

        /// <summary>
        /// データ初期化が完了したときに発行されるイベント。
        /// </summary>
        public static event System.Action OnDataInitialized;

        // JSONファイル名は不要になり、代わりに構築済みDBファイルをコピーする
        [SerializeField] private string dbFileName = "cards.db";

        private void Awake()
        {
            // シングルトンパターンの実装
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                // シーンをまたいでもこのオブジェクトが破棄されないようにする
                DontDestroyOnLoad(gameObject);
            }
        }

        // Startメソッドはコルーチンとして定義されている。
        // これは、ファイルI/Oやネットワーク通信などの時間のかかる非同期処理を、
        // メインスレッドをブロックせずに行うため。
        IEnumerator Start()
        {
            Debug.Log("AppManager: データの初期化を開始します...");

            // ランタイムで使用するパス（PersistentDataPath）で初期化
            CardDataImporter.Initialize(dbFileName);
            
            // ディレクトリパスの設定（WeissSchwarzサブディレクトリを使用）
            string subDir = "WeissSchwarz";
            string targetDir = Path.Combine(Application.persistentDataPath, subDir);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            // パスの構築
            string targetDbPath = Path.Combine(targetDir, dbFileName);
            string sourceDbPath = Path.Combine(Application.streamingAssetsPath, subDir, dbFileName);

            // TODO: 本番ではファイルのタイムスタンプやバージョン番号をチェックして、必要な場合のみコピーするように最適化する。
            // 今回はシンプルに、DBが存在しない場合、またはStreamingAssets版の方が新しい場合にコピーする実装とする。
            
            bool needCopy = false;

            if (!File.Exists(targetDbPath))
            {
                needCopy = true;
                Debug.Log("DBファイルが存在しないため、コピーを行います。");
            }
            else
            {
                // 簡易的な更新チェック: 常に上書き（開発中は便利だが、ユーザーデータとしてデッキ等を同じDBに保存している場合は注意が必要）
                // 読み取り専用マスターDBと、ユーザーデータDBを分けるのが一般的だが、今回はファイルをコピーして使用する。
                needCopy = true; 
            }

            if (needCopy)
            {
                // AndroidのStreamingAssetsは圧縮ファイル内にあるため、UnityWebRequestで読み出す必要がある
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest www = UnityWebRequest.Get(sourceDbPath))
                    {
                        yield return www.SendWebRequest();

                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            File.WriteAllBytes(targetDbPath, www.downloadHandler.data);
                            Debug.Log($"AppManager: DBをStreamingAssetsからコピーしました。\nSrc: {sourceDbPath}\nDst: {targetDbPath}");
                        }
                        else
                        {
                            // ファイルがない場合は致命的エラーだが、開発中ならエディタツールでの生成忘れの可能性がある
                            Debug.LogError($"AppManager: StreamingAssetsにDBファイルが見つかりません。エディタで [Tools > Generate DB] を実行してください。\nError: {www.error}");
                        }
                    }
                }
                else
                {
                    // iOS/Editor/StandaloneではSystem.IOが使える（ただしAndroid以外でもStreamingAssetsはFile.Copyでいける場合とWebRequest推奨の場合がある）
                    // 確実性を重視して、パスが存在する場合のみコピー
                    if (File.Exists(sourceDbPath))
                    {
                        File.Copy(sourceDbPath, targetDbPath, true);
                        Debug.Log($"AppManager: DBをコピーしました。");
                    }
                    else
                    {
                         Debug.LogError($"AppManager: StreamingAssetsにDBファイルが見つかりません: {sourceDbPath}");
                    }
                }
            }

            Debug.Log("AppManager: データの初期化が完了しました。");

            // データ準備完了をサブスクライバー（他のモジュール）に通知する
            OnDataInitialized?.Invoke();

            // 初期化後にメインのゲームシーンに遷移するなどの処理をここに追加できる
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}
