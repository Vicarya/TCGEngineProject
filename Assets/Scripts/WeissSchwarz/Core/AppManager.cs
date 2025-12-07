using System.Collections;
using System.IO;
using UnityEngine;
using TCG.Weiss.Data;
using UnityEngine.Networking; // For UnityWebRequest

namespace TCG.Weiss
{
    /// <summary>
    /// Manages application-wide data initialization, including downloading and importing card data.
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        public static event System.Action OnDataInitialized;

        [SerializeField] private string cardDataJsonFileName = "weiss_schwarz_cards.json";
        [SerializeField] private string dbFileName = "cards.db";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep this manager alive across scenes
            }
        }

        IEnumerator Start()
        {
            Debug.Log("AppManager: Initializing data...");

            // Initialize CardDataImporter with the database file name
            CardDataImporter.Initialize(dbFileName);

            // Check if the database exists and has data. For simplicity, we'll always try to import for now.
            // In a real app, you'd have version checks or only import if DB is empty/outdated.

            // Load JSON from StreamingAssets (or download from URL in a real app)
            string jsonFilePath = Path.Combine(Application.streamingAssetsPath, cardDataJsonFileName);
            string jsonString = "";

            // Use UnityWebRequest for platform compatibility (especially for Android/iOS StreamingAssets)
            using (UnityWebRequest www = UnityWebRequest.Get(jsonFilePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    jsonString = www.downloadHandler.text;
                    Debug.Log("AppManager: Card data JSON loaded from StreamingAssets.");
                }
                else
                {
                    Debug.LogError($"AppManager: Failed to load card data JSON from StreamingAssets: {www.error}");
                    // Fallback or error handling
                }
            }

            if (!string.IsNullOrEmpty(jsonString))
            {
                // Import data into SQLite
                CardDataImporter.ImportJsonToDatabase(jsonString);
            }
            else
            {
                Debug.LogWarning("AppManager: No JSON data to import.");
            }

            Debug.Log("AppManager: Data initialization complete.");

            // Notify subscribers that data is ready
            OnDataInitialized?.Invoke();

            // You might want to load your main game scene here after initialization
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}
