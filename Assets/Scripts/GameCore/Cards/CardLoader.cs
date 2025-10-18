using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Core
{
    public static class CardLoader
    {
        public static List<WeissCardData> AllCards { get; private set; } = new List<WeissCardData>();

        /// <summary>
        /// JSONファイルからカードデータを読み込み、ゲーム内で使用できる形式に変換します。
        /// </summary>
        /// <param name="filePath">カードデータJSONファイルのパス</param>
        public static void LoadCards(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Card data file not found at: {filePath}");
                return;
            }

            string json = File.ReadAllText(filePath);
            
            // 1. JSONを中間データ構造にデシリアライズ
            JsonCardCollection jsonCardCollection = JsonUtility.FromJson<JsonCardCollection>(json);
            
            var loadedCards = new List<WeissCardData>();

            // 2. 中間データから既存のWeissCardDataへマッピング
            foreach (var jsonCard in jsonCardCollection.cards)
            {
                var descriptionItems = new List<string>();
                if (jsonCard.traits != null)
                {
                    descriptionItems.AddRange(jsonCard.traits);
                }
                if (!string.IsNullOrEmpty(jsonCard.flavor))
                {
                    descriptionItems.Add(jsonCard.flavor);
                }

                var cardData = new WeissCardData
                {
                    // --- Base CardData Fields ---
                    CardGuid = System.Guid.NewGuid().ToString(),
                    CardCode = jsonCard.cardCode,
                    Name = jsonCard.name,
                    Set = jsonCard.set,
                    Rarity = jsonCard.rarity,
                    Atrribute = jsonCard.color, // WeissではColorをAtrributeとして扱う
                    Description = descriptionItems.ToArray(),
                    Illustration = jsonCard.illustrator,
                    ImagePath = $"{jsonCard.cardCode.Replace("/", "_")}.png", // パスを生成

                    // --- WeissCardData Fields ---
                    Level = jsonCard.level,
                    Cost = jsonCard.cost,
                    Power = jsonCard.power,
                    Soul = jsonCard.soul,
                    CardType = jsonCard.cardType,
                    TriggerIcon = jsonCard.trigger,
                    
                    // --- Metadata for extensibility ---
                    Metadata = new Dictionary<string, object>()
                };

                // 能力テキストをメタデータに格納
                if (jsonCard.text != null && jsonCard.text.Any())
                {
                    cardData.Metadata["ability_text"] = jsonCard.text;
                }
                
                loadedCards.Add(cardData);
            }

            AllCards = loadedCards;
            Debug.Log($"Loaded {AllCards.Count} cards from {filePath}");
        }

        #region Intermediate JSON Data Structures
        // JSONのルートオブジェクトに対応
        [System.Serializable]
        private class JsonCardCollection
        {
            public List<JsonCard> cards;
        }

        // JSONの各カードオブジェクトに対応
        [System.Serializable]
        private class JsonCard
        {
            public string cardCode;
            public string name;
            public string set;
            public string rarity;
            public string cardType;
            public string color;
            public int level;
            public int cost;
            public int power;
            public int soul;
            public string trigger;
            public List<string> traits;
            public List<string> text;
            public string flavor;
            public string illustrator;
        }
        #endregion
    }
}
