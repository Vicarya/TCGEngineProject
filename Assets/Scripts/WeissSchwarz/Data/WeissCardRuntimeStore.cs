using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TCG.Weiss.Data
{
    /// <summary>
    /// WeissカードRuntimeデータアクセスの入口。
    /// 呼び出し側はこのクラス経由でRepositoryを利用する。
    /// </summary>
    public static class WeissCardRuntimeStore
    {
        private static string _connectionString;
        private static WeissCardRuntimeRepository _repository;

        public static string ConnectionString
        {
            get
            {
                EnsureInitialized();
                return _connectionString;
            }
        }

        public static void Initialize(string dbFileName)
        {
            string dirPath = Path.Combine(Application.persistentDataPath, "WeissSchwarz");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            _connectionString = $"URI=file:{Path.Combine(dirPath, dbFileName)}";
            _repository = new WeissCardRuntimeRepository(_connectionString);
        }

        public static List<WeissCardData> LoadAll()
        {
            EnsureInitialized();
            return _repository.LoadAll();
        }

        public static void SaveAll(IEnumerable<WeissCardData> cards)
        {
            EnsureInitialized();
            _repository.SaveAll(cards);
        }

        private static void EnsureInitialized()
        {
            if (_repository != null)
            {
                return;
            }

            Initialize("cards.db");
        }
    }
}
