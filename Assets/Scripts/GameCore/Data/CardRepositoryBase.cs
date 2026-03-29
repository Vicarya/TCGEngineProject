using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace TCG.Core
{
    /// <summary>
    /// Repository共通フローを提供する抽象基底。
    /// 派生側は「クエリ/マッピング/スキーマ/UPSERT」だけ定義する。
    /// </summary>
    public abstract class CardRepositoryBase<TCardData> : ICardRepository<TCardData>
        where TCardData : CardData
    {
        protected string ConnectionString { get; }

        protected CardRepositoryBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 全件ロード。Select文とReader->Model変換は派生実装を使用する。
        /// </summary>
        public virtual List<TCardData> LoadAll()
        {
            var loader = new CardLoader<TCardData>(ConnectionString);
            return loader.Load(GetSelectAllQuery(), MapReader);
        }

        /// <summary>
        /// 一括保存。スキーマ補完後にトランザクションでUPSERTする。
        /// </summary>
        public virtual void SaveAll(IEnumerable<TCardData> cards)
        {
            if (cards == null)
            {
                return;
            }

            using var dbConnection = new SqliteConnection(ConnectionString);
            dbConnection.Open();

            using var dbCommand = dbConnection.CreateCommand();
            EnsureSchema(dbCommand);

            using var transaction = dbConnection.BeginTransaction();
            dbCommand.Transaction = transaction;

            foreach (var card in cards)
            {
                if (card == null)
                {
                    continue;
                }

                dbCommand.Parameters.Clear();
                BuildUpsertCommand(dbCommand, card);
                dbCommand.ExecuteNonQuery();
            }

            transaction.Commit();
            dbConnection.Close();
        }

        /// <summary>
        /// 単体保存。共通化のためSaveAllに委譲する。
        /// </summary>
        public virtual void SaveOne(TCardData card)
        {
            if (card == null)
            {
                return;
            }

            SaveAll(new[] { card });
        }

        protected abstract string GetSelectAllQuery();
        protected abstract TCardData MapReader(IDataReader reader);
        protected abstract void EnsureSchema(SqliteCommand command);
        protected abstract void BuildUpsertCommand(SqliteCommand command, TCardData card);
    }
}
