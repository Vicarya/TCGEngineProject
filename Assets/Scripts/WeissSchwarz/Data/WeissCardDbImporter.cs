using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TCG.Weiss.Data
{
    /// <summary>
    /// Weissカードデータのインポート専用ツール。
    /// Runtimeでのロード/セーブは WeissCardRuntimeStore / WeissCardRuntimeRepository に委譲する。
    /// </summary>
    public static class WeissCardDbImporter
    {
        private class CardDataWrapper
        {
            public List<WeissCardData> cards { get; set; }
        }

        private static readonly Dictionary<string, string> ColorMap = new()
        {
            ["黄"] = "Yellow",
            ["緑"] = "Green",
            ["赤"] = "Red",
            ["青"] = "Blue",
            ["紫"] = "Purple",
            ["Yellow"] = "Yellow",
            ["Green"] = "Green",
            ["Red"] = "Red",
            ["Blue"] = "Blue",
            ["Purple"] = "Purple"
        };

        private static readonly Dictionary<string, string> CardTypeMap = new()
        {
            ["キャラ"] = "Character",
            ["キャラクター"] = "Character",
            ["Character"] = "Character",
            ["イベント"] = "Event",
            ["Event"] = "Event",
            ["クライマックス"] = "Climax",
            ["Climax"] = "Climax"
        };

        private static readonly Dictionary<string, string> SideMap = new()
        {
            ["ヴァイス"] = "Weiss",
            ["Weiss"] = "Weiss",
            ["シュヴァルツ"] = "Schwarz",
            ["Schwarz"] = "Schwarz"
        };

        private static List<WeissCardData> DeserializeCardDataList(string jsonContent)
        {
            JToken rootToken = JToken.Parse(jsonContent);
            JArray cardsArray;

            if (rootToken.Type == JTokenType.Array)
            {
                cardsArray = (JArray)rootToken;
            }
            else if (rootToken.Type == JTokenType.Object)
            {
                CardDataWrapper wrapper = rootToken.ToObject<CardDataWrapper>();
                if (wrapper?.cards != null)
                {
                    return wrapper.cards;
                }

                cardsArray = ((JObject)rootToken)["cards"] as JArray;
                if (cardsArray == null)
                {
                    throw new JsonException("JSON配列、または cards 配列を含むオブジェクトを指定してください。");
                }
            }
            else
            {
                throw new JsonException("JSON配列、または cards 配列を含むオブジェクトを指定してください。");
            }

            var cards = new List<WeissCardData>(cardsArray.Count);
            foreach (JToken cardToken in cardsArray)
            {
                if (cardToken is not JObject cardObject)
                {
                    throw new JsonException("cards の各要素は JSON オブジェクトである必要があります。");
                }

                cards.Add(ParseCardData(cardObject));
            }

            return cards;
        }

        private static WeissCardData ParseCardData(JObject cardObject)
        {
            WeissCardData cardData = new()
            {
                CardCode = GetString(cardObject, "cardCode", "card_no"),
                Name = GetString(cardObject, "name"),
                WorkId = GetString(cardObject, "workId", "作品ID", "作品コード"),
                DetailPageUrl = GetString(cardObject, "detailPageUrl", "detail_page_url"),
                ImagePath = GetString(cardObject, "imageUrl", "image_url"),
                Side = NormalizeSide(GetString(cardObject, "side", "サイド")),
                CardType = NormalizeCardType(GetString(cardObject, "cardType", "type", "種類")),
                Level = GetInt(cardObject, "level", "レベル") ?? 0,
                Cost = GetInt(cardObject, "cost", "コスト") ?? 0,
                Power = GetInt(cardObject, "power", "パワー") ?? 0,
                Soul = GetInt(cardObject, "soul", "ソウル") ?? 0,
                Color = NormalizeColor(GetString(cardObject, "color", "色")),
                Rarity = GetString(cardObject, "rarity", "レアリティ"),
                TriggerIcon = NormalizeTrigger(GetString(cardObject, "trigger", "triggerIcon", "トリガー")),
                Traits = GetStringList(cardObject, "traits", "features", "特徴") ?? new List<string>(),
                FlavorText = GetString(cardObject, "flavorText", "flavor_text", "flavor", "フレーバー"),
                Abilities = GetStringList(cardObject, "abilities", "text", "能力", "テキスト") ?? new List<string>()
            };

            cardData.EnsureRuntimeDefaults();
            return cardData;
        }

        private static string GetString(JObject cardObject, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (!cardObject.TryGetValue(propertyName, out JToken token) || token.Type == JTokenType.Null)
                {
                    continue;
                }

                if (token.Type == JTokenType.String)
                {
                    return token.Value<string>();
                }

                if (token.Type is JTokenType.Integer or JTokenType.Float or JTokenType.Boolean)
                {
                    return token.ToString();
                }
            }

            return null;
        }

        private static int? GetInt(JObject cardObject, params string[] propertyNames)
        {
            string value = GetString(cardObject, propertyNames);
            return int.TryParse(value, out int parsedValue) ? parsedValue : null;
        }

        private static List<string> GetStringList(JObject cardObject, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (!cardObject.TryGetValue(propertyName, out JToken token) || token.Type == JTokenType.Null)
                {
                    continue;
                }

                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<List<string>>() ?? new List<string>();
                }

                if (token.Type == JTokenType.String)
                {
                    string value = token.Value<string>();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return new List<string> { value };
                    }
                }
            }

            return null;
        }

        private static string NormalizeColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return color;
            }

            return ColorMap.TryGetValue(color, out string normalizedColor) ? normalizedColor : color;
        }

        private static string NormalizeCardType(string cardType)
        {
            if (string.IsNullOrWhiteSpace(cardType))
            {
                return cardType;
            }

            return CardTypeMap.TryGetValue(cardType, out string normalizedCardType) ? normalizedCardType : cardType;
        }

        private static string NormalizeSide(string side)
        {
            if (string.IsNullOrWhiteSpace(side))
            {
                return side;
            }

            return SideMap.TryGetValue(side, out string normalizedSide) ? normalizedSide : side;
        }

        private static string NormalizeTrigger(string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger) ||
                trigger == "-" ||
                trigger == ".." ||
                trigger == "・" ||
                trigger == "なし" ||
                trigger == "無し")
            {
                return "None";
            }

            return trigger;
        }

        public static void ImportJsonToDatabase(string jsonContent, string overrideDbPath = null)
        {
            List<WeissCardData> cardDataList;
            try
            {
                cardDataList = DeserializeCardDataList(jsonContent);
            }
            catch (JsonException e)
            {
                Debug.LogError($"JSONの解析に失敗しました: {e.Message}");
                return;
            }

            if (cardDataList == null || cardDataList.Count == 0)
            {
                Debug.LogWarning("JSON内にカードデータが見つかりませんでした。");
                return;
            }

            string targetDbPath = overrideDbPath;
            if (string.IsNullOrEmpty(targetDbPath))
            {
                WeissCardRuntimeStore.Initialize("cards.db");
                targetDbPath = WeissCardRuntimeStore.ConnectionString;
            }

            var repository = new WeissCardRuntimeRepository(targetDbPath);
            repository.SaveAll(cardDataList);

            Debug.Log($"カードデータ {cardDataList.Count} 件をSQLite DBに取り込みました: {targetDbPath}");
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Weiss Schwarz/Generate DB from JSON")]
        public static void GenerateDatabaseInEditor()
        {
            string subDir = "WeissSchwarz";
            string jsonFileName = "cards.json";
            string dbFileName = "cards.db";

            string streamingAssetsPath = Application.streamingAssetsPath;
            string workDir = Path.Combine(streamingAssetsPath, subDir);

            string jsonPath = Path.Combine(workDir, jsonFileName);
            string dbPath = Path.Combine(workDir, dbFileName);
            string dbConnectionPath = $"URI=file:{dbPath}";

            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSONファイルが見つかりません: {jsonPath}");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            ImportJsonToDatabase(jsonContent, dbConnectionPath);

            AssetDatabase.Refresh();
        }
#endif
    }
}
