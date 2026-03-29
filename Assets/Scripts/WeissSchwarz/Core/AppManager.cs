using System.Collections;
using System.IO;
using TCG.Weiss.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace TCG.Weiss
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        public static event System.Action OnDataInitialized;

        [SerializeField] private string dbFileName = "cards.db";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private static string BuildPersistentDbPath(string dbFileName)
        {
            string targetDir = Path.Combine(Application.persistentDataPath, "WeissSchwarz");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            return Path.Combine(targetDir, dbFileName);
        }

        private static string BuildStreamingAssetsDbPath(string dbFileName)
        {
            return Path.Combine(Application.streamingAssetsPath, "WeissSchwarz", dbFileName);
        }

        IEnumerator Start()
        {
            Debug.Log("AppManager: starting data initialization.");

            WeissCardRuntimeStore.Initialize(dbFileName);

            string targetDbPath = BuildPersistentDbPath(dbFileName);
            string sourceDbPath = BuildStreamingAssetsDbPath(dbFileName);

            Debug.Log($"AppManager: Source DB Path = {sourceDbPath}");
            Debug.Log($"AppManager: Target DB Path = {targetDbPath}");

            bool copySucceeded = false;

            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(sourceDbPath))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(targetDbPath, www.downloadHandler.data);
                        copySucceeded = true;
                    }
                    else
                    {
                        Debug.LogError($"AppManager: Failed to copy DB from StreamingAssets. Error: {www.error}");
                    }
                }
            }
            else
            {
                if (File.Exists(sourceDbPath))
                {
                    File.Copy(sourceDbPath, targetDbPath, true);
                    copySucceeded = true;
                }
                else
                {
                    Debug.LogError($"AppManager: StreamingAssets DB not found: {sourceDbPath}");
                }
            }

            if (!copySucceeded)
            {
                if (File.Exists(targetDbPath))
                {
                    File.Delete(targetDbPath);
                    Debug.LogWarning($"AppManager: Deleted stale persistent DB: {targetDbPath}");
                }
                else
                {
                    Debug.LogWarning("AppManager: No persistent DB to delete.");
                }
            }
            else
            {
                Debug.Log("AppManager: DB copied to persistentDataPath.");
            }

            Debug.Log("AppManager: data initialization completed.");
            OnDataInitialized?.Invoke();
        }
    }
}
