using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using TCG.Weiss;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        
        // パスが未設定の場合のフォールバック（WeissCardDatabaseからの呼び出し用）
        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dbPath)) Initialize("cards.db");
                return _dbPath;
            }
        }

        /// <summary>
        /// データベースへの接続パスを初期化します。
        /// </summary>
        /// <param name="dbFileName">SQLiteデータベースのファイル名（例: "cards.db"）。</param>
        public static void Initialize(string dbFileName)
        {
            // サブディレクトリ "WeissSchwarz" を利用して整理する
            string dirPath = System.IO.Path.Combine(Application.persistentDataPath, "WeissSchwarz");
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            // データベースのパスを設定
            _dbPath = $"URI=file:{System.IO.Path.Combine(dirPath, dbFileName)}";
            Debug.Log($"SQLite DB Path: {_dbPath}");
        }

        /// <summary>
        /// JSON文字列からカードデータを読み込み、SQLiteデータベースにインポートします。
        /// テーブルが存在しない場合は自動的に作成します。
        /// </summary>
        /// <param name="jsonContent">カードデータが含まれるJSON文字列。</param>
        /// <param name="overrideDbPath">保存先のDBパスを指定する場合に使用（nullならデフォルト）</param>
        
        // JSONのルート要素 {"cards": [...]} に対応するためのラッパークラス
        private class CardDataWrapper
        {
            public List<WeissCardData> cards { get; set; }
        }

        public static void ImportJsonToDatabase(string jsonContent, string overrideDbPath = null)
        {
            string targetDbPath = overrideDbPath ?? DbPath;
            if (string.IsNullOrEmpty(targetDbPath))
            {
                // ターゲットパスが指定されておらず、Initializeもされていない場合
                Initialize("cards.db");
                targetDbPath = DbPath;
            }

            // 1. JSON文字列をWeissCardDataオブジェクトのリストにデシリアライズ
            List<WeissCardData> cardDataList;
            try
            {
                // まず {"cards": [...]} 形式としてデシリアライズを試みる
                var wrapper = JsonConvert.DeserializeObject<CardDataWrapper>(jsonContent);
                if (wrapper != null && wrapper.cards != null)
                {
                    cardDataList = wrapper.cards;
                }
                else
                {
                    // 失敗した場合は、旧形式の配列 [...] として試みる
                    cardDataList = JsonConvert.DeserializeObject<List<WeissCardData>>(jsonContent);
                }
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
            using (var dbConnection = new SqliteConnection(targetDbPath))
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

                    // トランザクションを開始して一括処理を行う（パフォーマンス向上）
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        // コマンドをトランザクションに関連付ける（重要）
                        dbCommand.Transaction = transaction;

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
                        transaction.Commit();
                    }
                }
                dbConnection.Close();
            }
            Debug.Log($"SQLiteデータベース({targetDbPath})に{cardDataList.Count}枚のカードを正常にインポートしました。");
        }

        /// <summary>
        /// SQLiteデータベースからすべてのカードデータを取得します。
        /// </summary>
        /// <returns>データベース内のすべてのカードデータを含むWeissCardDataのリスト。</returns>
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

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用: StreamingAssetsにあるJSONを読み込み、同じ場所にSQLiteデータベースを生成する。
        /// メニュー [Tools > Weiss Schwarz > Generate DB from JSON] から実行可能。
        /// </summary>
        [MenuItem("Tools/Weiss Schwarz/Generate DB from JSON")]
        public static void GenerateDatabaseInEditor()
        {
            string subDir = "WeissSchwarz";
            string jsonFileName = "cards.json"; // 実際のファイル名に修正
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
            
            AssetDatabase.Refresh(); // エディタ上の表示を更新
        }
#endif
    }
}
