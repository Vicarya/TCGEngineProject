using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

namespace TCG.Core
{
    /// <summary>
    /// データベースから特定の型のカードデータをロードするための汎用的なクラス。
    /// </summary>
    public class CardLoader<TCard> where TCard : CardData
    {
        private readonly string _connectionString;

        /// <summary>
        /// コンストラクタで接続文字列（DBのパス）を受け取ります。
        /// </summary>
        /// <param name="connectionString">SQLite接続文字列</param>
        public CardLoader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 指定されたクエリを実行し、マッピング関数を用いてデータをロードします。
        /// </summary>
        /// <param name="query">実行するSELECTクエリ</param>
        /// <param name="mapFunction">IDataReaderの各行をTCardインスタンスに変換するデリゲート</param>
        /// <returns>ロードされたカードデータのリスト</returns>
        public List<TCard> Load(string query, Func<IDataReader, TCard> mapFunction)
        {
            var results = new List<TCard>();

            if (string.IsNullOrEmpty(_connectionString))
            {
                Debug.LogError($"[{typeof(TCard).Name} Loader] Connection string is null or empty.");
                return results;
            }

            try
            {
                using (var dbConnection = new SqliteConnection(_connectionString))
                {
                    dbConnection.Open();
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.CommandText = query;
                        using (IDataReader reader = dbCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TCard card = mapFunction(reader);
                                if (card != null) results.Add(card);
                            }
                        }
                    }
                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{typeof(TCard).Name} Loader] Database load error: {ex.Message}");
            }

            return results;
        }
    }
}