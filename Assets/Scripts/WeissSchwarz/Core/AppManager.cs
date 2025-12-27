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

        [SerializeField] private string cardDataJsonFileName = "weiss_schwarz_cards.json";
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

            // CardDataImporterにデータベースファイル名を渡して初期化
            CardDataImporter.Initialize(dbFileName);

            // 本来のアプリケーションでは、DBのバージョンをチェックしたり、
            // DBが空または古い場合のみインポートを実行したりするべき。
            // ここでは簡潔さのために、毎回インポートを試みる。

            // StreamingAssetsからJSONファイルを読み込む
            string jsonFilePath = Path.Combine(Application.streamingAssetsPath, cardDataJsonFileName);
            string jsonString = "";

            // File.ReadAllTextではなくUnityWebRequestを使用する。
            // これにより、AndroidやiOSなどの異なるプラットフォームでもStreamingAssetsに正しくアクセスできる。
            using (UnityWebRequest www = UnityWebRequest.Get(jsonFilePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonString = www.downloadHandler.text;
                    Debug.Log("AppManager: StreamingAssetsからカードデータJSONを読み込みました。");
                }
                else
                {
                    Debug.LogError($"AppManager: StreamingAssetsからのカードデータJSONの読み込みに失敗しました: {www.error}");
                    // エラーハンドリングまたはフォールバック処理
                }
            }

            if (!string.IsNullOrEmpty(jsonString))
            {
                // JSONデータをSQLiteデータベースにインポートする
                CardDataImporter.ImportJsonToDatabase(jsonString);
            }
            else
            {
                Debug.LogWarning("AppManager: インポートするJSONデータがありません。");
            }

            Debug.Log("AppManager: データの初期化が完了しました。");

            // データ準備完了をサブスクライバー（他のモジュール）に通知する
            OnDataInitialized?.Invoke();

            // 初期化後にメインのゲームシーンに遷移するなどの処理をここに追加できる
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}
