using GameCore.Database;
using TCG.Weiss;
using UnityEngine;
using System.Linq;
using TCG.Weiss.Data; // CardDataImporterを使用するために追加

// NOTE: namespaceがファイルの物理パスと一致していません (WeissSchwarz.Database vs. Assets/Scripts/WeissSchwarz/Core)。リファクタリング中の可能性があります。
namespace WeissSchwarz.Database
{
    /// <summary>
    /// ヴァイスシュヴァルツのカードデータに特化したデータベース。
    /// 汎用的な CardDatabase を継承し、具体的なデータロード処理を実装しています。
    /// シングルトンとして設計されており、ゲーム内のどこからでもアクセス可能です。
    /// </summary>
    public class WeissCardDatabase : CardDatabase<WeissCardData, WeissCardQuery>
    {
        /// <summary>
        /// データベースのシングルトンインスタンス。
        /// </summary>
        public static WeissCardDatabase Instance { get; private set; }

        /// <summary>
        /// オブジェクト初期化時に呼び出され、シングルトンインスタンスを設定し、データベースをロードします。
        /// </summary>
        private void Awake()
        {
            Instance = this;
            LoadDatabase();
        }

        /// <summary>
        /// データベースにすべてのカードデータをロードします。
        /// </summary>
        public override void LoadDatabase()
        {
            
            // ScriptableObjectを経由せず、SQLiteから直接データをロードする
            allCards = CardDataImporter.GetAllCardData();

            if (allCards.Count > 0)
            {
                Debug.Log($"WeissCardDatabase: {allCards.Count}枚のカードがロードされました。");
            }
            else
            {
                Debug.LogWarning("WeissCardDatabase: カードデータが0件です。JSONのインポートを行ってください。");
            }
        }
    }
}