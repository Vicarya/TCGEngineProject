using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TCG.Weiss;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TCG.Weiss.Data
{
    public static class CardDataImporter
    {
        private static string _dbPath;

        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dbPath))
                {
                    Initialize("cards.db");
                }

                return _dbPath;
            }
        }

        private class CardDataWrapper
        {
            public List<WeissCardData> cards { get; set; }
        }

        public static void Initialize(string dbFileName)
        {
            string dirPath = System.IO.Path.Combine(Application.persistentDataPath, "WeissSchwarz");
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            _dbPath = $"URI=file:{System.IO.Path.Combine(dirPath, dbFileName)}";
            Debug.Log($"SQLite DB Path: {_dbPath}");
        }

        private static List<WeissCardData> DeserializeCardDataList(string jsonContent)
        {
            JToken rootToken = JToken.Parse(jsonContent);

            if (rootToken.Type == JTokenType.Array)
            {
                return rootToken.ToObject<List<WeissCardData>>();
            }

            if (rootToken.Type == JTokenType.Object)
            {
                CardDataWrapper wrapper = rootToken.ToObject<CardDataWrapper>();
                if (wrapper?.cards != null)
                {
                    return wrapper.cards;
                }
            }

            throw new JsonException("Expected either a JSON array or an object containing a 'cards' array.");
        }

        public static void ImportJsonToDatabase(string jsonContent, string overrideDbPath = null)
        {
            string targetDbPath = overrideDbPath ?? DbPath;
            if (string.IsNullOrEmpty(targetDbPath))
            {
                Initialize("cards.db");
                targetDbPath = DbPath;
            }

            List<WeissCardData> cardDataList;
            try
            {
                cardDataList = DeserializeCardDataList(jsonContent);
            }
            catch (JsonException e)
            {
                Debug.LogError($"JSONのデシリアライズに失敗しました: {e.Message}");
                return;
            }

            if (cardDataList == null || cardDataList.Count == 0)
            {
                Debug.LogWarning("JSONコンテンツにカードデータが見つかりませんでした。");
                return;
            }

            using (var dbConnection = new SqliteConnection(targetDbPath))
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS cards (
                            card_no TEXT PRIMARY KEY,
                            name TEXT,
                            detail_page_url TEXT,
                            image_url TEXT,
                            side TEXT,
                            type TEXT,
                            level TEXT,
                            color TEXT,
                            power TEXT,
                            soul TEXT,
                            cost TEXT,
                            rarity TEXT,
                            trigger TEXT,
                            features TEXT,
                            flavor_text TEXT,
                            abilities TEXT
                        );";
                    dbCommand.ExecuteNonQuery();

                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        dbCommand.Transaction = transaction;

                        foreach (var cardData in cardDataList)
                        {
                            dbCommand.CommandText = @"
                                INSERT OR REPLACE INTO cards (
                                    card_no, name, detail_page_url, image_url, side, type, level, color, power, soul, rarity, trigger, features, flavor_text, abilities, cost
                                ) VALUES (
                                    @card_no, @name, @detail_page_url, @image_url, @side, @type, @level, @color, @power, @soul, @rarity, @trigger, @features, @flavor_text, @abilities, @cost
                                );";

                            dbCommand.Parameters.Clear();
                            dbCommand.Parameters.AddWithValue("@card_no", cardData.CardCode);
                            dbCommand.Parameters.AddWithValue("@name", cardData.Name);
                            dbCommand.Parameters.AddWithValue("@detail_page_url", cardData.detail_page_url);
                            dbCommand.Parameters.AddWithValue("@image_url", cardData.ImagePath);
                            dbCommand.Parameters.AddWithValue("@side", cardData.Side ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@type", cardData.CardType ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@level", cardData.Level.ToString());
                            dbCommand.Parameters.AddWithValue("@color", cardData.Color ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@power", cardData.Power.ToString());
                            dbCommand.Parameters.AddWithValue("@soul", cardData.Soul.ToString());
                            dbCommand.Parameters.AddWithValue("@cost", cardData.Cost.ToString());
                            dbCommand.Parameters.AddWithValue("@rarity", cardData.Rarity ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@trigger", cardData.TriggerIcon ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@features", JsonConvert.SerializeObject(cardData.Traits ?? new List<string>()));
                            dbCommand.Parameters.AddWithValue("@flavor_text", cardData.FlavorText ?? string.Empty);
                            dbCommand.Parameters.AddWithValue("@abilities", JsonConvert.SerializeObject(cardData.Abilities ?? new List<string>()));

                            dbCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                dbConnection.Close();
            }

            Debug.Log($"SQLiteデータベース({targetDbPath})に{cardDataList.Count}件のカードを正常にインポートしました。");
        }

        public static List<WeissCardData> GetAllCardData()
        {
            var cardDataList = new List<WeissCardData>();
            if (string.IsNullOrEmpty(DbPath))
            {
                Debug.LogError("CardDataImporterが初期化されていません。先にInitialize()を呼び出してください。");
                return cardDataList;
            }

            using (var dbConnection = new SqliteConnection(DbPath))
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = "SELECT * FROM cards;";
                    using (IDataReader reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var cardData = new WeissCardData
                            {
                                CardCode = reader["card_no"].ToString(),
                                Name = reader["name"].ToString(),
                                detail_page_url = reader["detail_page_url"].ToString(),
                                ImagePath = reader["image_url"].ToString(),
                                Side = reader["side"].ToString(),
                                CardType = reader["type"].ToString(),
                                Level = int.TryParse(reader["level"].ToString(), out var level) ? level : 0,
                                Color = reader["color"].ToString(),
                                Power = int.TryParse(reader["power"].ToString(), out var power) ? power : 0,
                                Soul = int.TryParse(reader["soul"].ToString(), out var soul) ? soul : 0,
                                Cost = int.TryParse(reader["cost"].ToString(), out var cost) ? cost : 0,
                                Rarity = reader["rarity"].ToString(),
                                TriggerIcon = reader["trigger"].ToString(),
                                FlavorText = reader["flavor_text"].ToString(),
                                Traits = JsonConvert.DeserializeObject<List<string>>(reader["features"].ToString()) ?? new List<string>(),
                                Abilities = JsonConvert.DeserializeObject<List<string>>(reader["abilities"].ToString()) ?? new List<string>()
                            };
                            cardDataList.Add(cardData);
                        }
                    }
                }

                dbConnection.Close();
            }

            Debug.Log($"SQLiteデータベースから{cardDataList.Count}件のカードを取得しました。");
            return cardDataList;
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Weiss Schwarz/Generate DB from JSON")]
        public static void GenerateDatabaseInEditor()
        {
            string subDir = "WeissSchwarz";
            string jsonFileName = "cards.json";
            string dbFileName = "cards.db";

            string streamingAssetsPath = Application.streamingAssetsPath;
            string workDir = System.IO.Path.Combine(streamingAssetsPath, subDir);

            string jsonPath = System.IO.Path.Combine(workDir, jsonFileName);
            string dbPath = System.IO.Path.Combine(workDir, dbFileName);
            string dbConnectionPath = $"URI=file:{dbPath}";

            if (!System.IO.File.Exists(jsonPath))
            {
                Debug.LogError($"JSONファイルが見つかりません: {jsonPath}");
                return;
            }

            string jsonContent = System.IO.File.ReadAllText(jsonPath);
            ImportJsonToDatabase(jsonContent, dbConnectionPath);

            AssetDatabase.Refresh();
        }
#endif
    }
}
