using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.Data
{
    /// <summary>
    /// Utility class for importing card data from JSON into an SQLite database.
    /// </summary>
    public static class CardDataImporter
    {
        private static string _dbPath;

        /// <summary>
        /// Initializes the database connection path.
        /// </summary>
        /// <param name="dbFileName">The name of the SQLite database file (e.g., "cards.db").</param>
        public static void Initialize(string dbFileName)
        {
            _dbPath = $"URI=file:{Application.persistentDataPath}/{dbFileName}";
            Debug.Log($"SQLite DB Path: {_dbPath}");
        }

        /// <summary>
        /// Imports card data from a JSON string into the SQLite database.
        /// Creates the table if it doesn't exist.
        /// </summary>
        /// <param name="jsonContent">The JSON string containing card data.</param>
        public static void ImportJsonToDatabase(string jsonContent)
        {
            if (string.IsNullOrEmpty(_dbPath))
            {
                Debug.LogError("CardDataImporter not initialized. Call Initialize() first.");
                return;
            }

            List<WeissCardData> cardDataList;
            try
            {
                cardDataList = JsonConvert.DeserializeObject<List<WeissCardData>>(jsonContent);
            }
            catch (JsonException e)
            {
                Debug.LogError($"Failed to deserialize JSON content: {e.Message}");
                return;
            }

            if (cardDataList == null || cardDataList.Count == 0)
            {
                Debug.LogWarning("No card data found in JSON content.");
                return;
            }

            using (var dbConnection = new SqliteConnection(_dbPath))
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    // Create table if not exists
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
                            features TEXT, -- Stored as JSON array
                            flavor_text TEXT,
                            abilities TEXT -- Stored as JSON array
                        );";
                    dbCommand.ExecuteNonQuery();

                    // Insert or Update data
                    foreach (var cardData in cardDataList)
                    {
                        dbCommand.CommandText = @"
                            INSERT OR REPLACE INTO cards (
                                card_no, name, detail_page_url, image_url, side, type, level, color, power, soul, rarity, trigger, features, flavor_text, abilities, cost
                            ) VALUES (
                                @card_no, @name, @detail_page_url, @image_url, @side, @type, @level, @color, @power, @soul, @rarity, @trigger, @features, @flavor_text, @abilities, @cost
                            );";
                        dbCommand.Parameters.Clear();
                        dbCommand.Parameters.AddWithValue("@card_no", cardData.card_no);
                        dbCommand.Parameters.AddWithValue("@name", cardData.name);
                        dbCommand.Parameters.AddWithValue("@detail_page_url", cardData.detail_page_url);
                        dbCommand.Parameters.AddWithValue("@image_url", cardData.image_url);
                        dbCommand.Parameters.AddWithValue("@side", cardData.サイド);
                        dbCommand.Parameters.AddWithValue("@type", cardData.種類);
                        dbCommand.Parameters.AddWithValue("@level", cardData.レベル);
                        dbCommand.Parameters.AddWithValue("@color", cardData.色);
                        dbCommand.Parameters.AddWithValue("@power", cardData.パワー);
                        dbCommand.Parameters.AddWithValue("@soul", cardData.ソウル);
                        dbCommand.Parameters.AddWithValue("@cost", cardData.コスト);
                        dbCommand.Parameters.AddWithValue("@rarity", cardData.レアリティ);
                        dbCommand.Parameters.AddWithValue("@trigger", cardData.トリガー);
                        dbCommand.Parameters.AddWithValue("@features", JsonConvert.SerializeObject(cardData.特徴)); // Serialize list to JSON
                        dbCommand.Parameters.AddWithValue("@flavor_text", cardData.flavor_text);
                        dbCommand.Parameters.AddWithValue("@abilities", JsonConvert.SerializeObject(cardData.abilities)); // Serialize list to JSON
                        dbCommand.ExecuteNonQuery();
                    }
                }
                dbConnection.Close();
            }
            Debug.Log($"Successfully imported {cardDataList.Count} cards into SQLite database.");
        }

        /// <summary>
        /// Retrieves all card data from the SQLite database.
        /// </summary>
        /// <returns>A list of WeissCardData objects.</returns>
        public static List<WeissCardData> GetAllCardData()
        {
            var cardDataList = new List<WeissCardData>();
            if (string.IsNullOrEmpty(_dbPath))
            {
                Debug.LogError("CardDataImporter not initialized. Call Initialize() first.");
                return cardDataList;
            }

            using (var dbConnection = new SqliteConnection(_dbPath))
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
                                card_no = reader["card_no"].ToString(),
                                name = reader["name"].ToString(),
                                detail_page_url = reader["detail_page_url"].ToString(),
                                image_url = reader["image_url"].ToString(),
                                サイド = reader["side"].ToString(),
                                種類 = reader["type"].ToString(),
                                レベル = reader["level"].ToString(),
                                色 = reader["color"].ToString(),
                                パワー = reader["power"].ToString(),
                                ソウル = reader["soul"].ToString(),
                                コスト = reader["cost"].ToString(),
                                レアリティ = reader["rarity"].ToString(),
                                トリガー = reader["trigger"].ToString(),
                                特徴 = JsonConvert.DeserializeObject<List<string>>(reader["features"].ToString()), // Deserialize JSON to list
                                flavor_text = reader["flavor_text"].ToString(),
                                abilities = JsonConvert.DeserializeObject<List<string>>(reader["abilities"].ToString()) // Deserialize JSON to list
                            };
                            cardDataList.Add(cardData);
                        }
                    }
                }
                dbConnection.Close();
            }
            Debug.Log($"Retrieved {cardDataList.Count} cards from SQLite database.");
            return cardDataList;
        }
    }
}
