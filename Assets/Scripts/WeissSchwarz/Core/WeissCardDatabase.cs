using GameCore.Database;
using TCG.Weiss;
using UnityEngine;
using System.Linq;

namespace WeissSchwarz.Database
{
    /// <summary>
    /// ヴァイスシュヴァルツのカードデータベース。
    /// </summary>
    public class WeissCardDatabase : CardDatabase<WeissCardData, WeissCardQuery>
    {
        // シングルトンインスタンス
        public static WeissCardDatabase Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            LoadDatabase();
        }

        public override void LoadDatabase()
        {
            // Resources/CardDataフォルダなどからすべてのWeissCardDataアセットをロード
            var cardAssets = Resources.LoadAll<WeissCardDataAsset>("CardData");
            allCards = cardAssets.Select(asset => asset.Data).ToList();
            Debug.Log($"{allCards.Count} cards loaded.");
        }
    }
}