using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Weiss.Data
{
    /// <summary>
    /// JSON形式のカードデータをSQLiteデータベースにインポート・エクスポートするための静的ユーティリティクラス。
    /// 外部ライブラリである `Newtonsoft.Json` と、UnityでSQLiteを扱うための `Mono.Data.Sqlite` を使用します。
    /// </summary>
    public static class CardDataImporter
    {
        // データベースファイルへの接続パス
        private static string _dbPath;

        /// <summary>
        /// データベースへの接続パスを初期化します。
        /// </summary>
        /// <param name="dbFileName">SQLiteデータベースのファイル名（例: "cards.db"）。</param>
        public static void Initialize(string dbFileName)
        {
            // データベースのパスを、デバイスの永続的なデータ保存領域に設定
            _dbPath = $"URI=file:{Application.persistentDataPath}/{dbFileName}";
            Debug.Log($"SQLite DB Path: {_dbPath}");
        }

        /// <summary>
        /// JSON文字列からカードデータを読み込み、SQLiteデータベースにインポートします。
        /// テーブルが存在しない場合は自動的に作成します。
        /// </summary>
        /// <param name="jsonContent">カードデータが含まれるJSON文字列。</param>
        public static void ImportJsonToDatabase(string jsonContent)
        {
            if (string.IsNullOrEmpty(_dbPath))
            {
                Debug.LogError("CardDataImporterが初期化されていません。先にInitialize()を呼び出してください。");
                return;
            }

            // 1. JSON文字列をWeissCardDataオブジェクトのリストにデシリアライズ
            List<WeissCardData> cardDataList;
            try
            {
                cardDataList = JsonConvert.DeserializeObject<List<WeissCardData>>(jsonContent);
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

            // 2. データベースに接続
            using (var dbConnection = new SqliteConnection(_dbPath))
            {
                dbConnection.Open();
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    // 3. テーブルが存在しない場合に備えて、CREATE TABLE文を実行
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
                            features TEXT, -- List<string>をJSON文字列として格納
                            flavor_text TEXT,
                            abilities TEXT -- List<string>をJSON文字列として格納
                        );";
                    dbCommand.ExecuteNonQuery();

                    // 4. 各カードデータをデータベースに挿入または更新
                    foreach (var cardData in cardDataList)
                    {
                        // `card_no`が既に存在する場合はレコードを更新し、存在しない場合は挿入する
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
                        dbCommand.Parameters.AddWithValue("@flavor_text", cardData.flavor_text);
                        
                        // List<string>型のプロパティをJSON文字列に変換してTEXTカラムに保存
                        dbCommand.Parameters.AddWithValue("@features", JsonConvert.SerializeObject(cardData.特徴));
                        dbCommand.Parameters.AddWithValue("@abilities", JsonConvert.SerializeObject(cardData.abilities));
                        
                        dbCommand.ExecuteNonQuery();
                    }
                }
                dbConnection.Close();
            }
            Debug.Log($"SQLiteデータベースに{cardDataList.Count}枚のカードを正常にインポートしました。");
        }

        /// <summary>
        /// SQLiteデータベースからすべてのカードデータを取得します。
        /// </summary>
        /// <returns>データベース内のすべてのカードデータを含むWeissCardDataのリスト。</returns>
        public static List<WeissCardData> GetAllCardData()
        {
            var cardDataList = new List<WeissCardData>();
            if (string.IsNullOrEmpty(_dbPath))
            {
                Debug.LogError("CardDataImporterが初期化されていません。先にInitialize()を呼び出してください。");
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
                                flavor_text = reader["flavor_text"].ToString(),
                                
                                // TEXTカラムから読み込んだJSON文字列をList<string>にデシリアライズして戻す
                                特徴 = JsonConvert.DeserializeObject<List<string>>(reader["features"].ToString()),
                                abilities = JsonConvert.DeserializeObject<List<string>>(reader["abilities"].ToString())
                            };
                            cardDataList.Add(cardData);
                        }
                    }
                }
                dbConnection.Close();
            }
            Debug.Log($"SQLiteデータベースから{cardDataList.Count}枚のカードを取得しました。");
            return cardDataList;
        }
    }
}
