using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Mono.Data.Sqlite;
using Newtonsoft.Json;

namespace TCG.Core
{
    /// <summary>
    /// モデル型からSQLiteのスキーマ/CRUD SQLを自動組み立てする基底クラス。
    /// 基本ケースはこのクラスで吸収し、特殊ケースのみ派生側で補う。
    /// </summary>
    public abstract class AutoSqliteCardRepositoryBase<TCardData> : CardRepositoryBase<TCardData>
        where TCardData : CardData, new()
    {
        private readonly string _tableName;
        private readonly string _primaryKeyColumn;
        private readonly IReadOnlyList<MemberMap> _members;

        protected AutoSqliteCardRepositoryBase(string connectionString, string tableName, string primaryKeyColumn)
            : base(connectionString)
        {
            _tableName = tableName;
            _primaryKeyColumn = primaryKeyColumn;
            _members = BuildMemberMaps();
        }

        /// <summary>
        /// 読み込み前に必ずスキーマ補完を走らせる。
        /// 旧DBを先に拡張してからSELECTするため、missing columnを防げる。
        /// </summary>
        public override List<TCardData> LoadAll()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                EnsureSchema(command);
                connection.Close();
            }

            return base.LoadAll();
        }

        protected override string GetSelectAllQuery()
        {
            string columns = string.Join(", ", _members.Select(m => $"[{m.ColumnName}]"));
            return $"SELECT {columns} FROM [{_tableName}];";
        }

        protected override TCardData MapReader(IDataReader reader)
        {
            var entity = new TCardData();

            foreach (var member in _members)
            {
                object value = reader[member.ColumnName];
                object converted = ConvertFromDb(value, member.MemberType);
                member.Set(entity, converted);
            }

            OnEntityLoaded(entity);
            return entity;
        }

        protected override void EnsureSchema(SqliteCommand command)
        {
            EnsureTable(command);

            HashSet<string> existingColumns = GetExistingColumns(command);
            foreach (var member in _members)
            {
                if (existingColumns.Contains(member.ColumnName))
                {
                    continue;
                }

                command.Parameters.Clear();
                command.CommandText = $"ALTER TABLE [{_tableName}] ADD COLUMN [{member.ColumnName}] {member.SqliteType};";
                command.ExecuteNonQuery();
                existingColumns.Add(member.ColumnName);
            }

            OnSchemaEnsured(command, existingColumns);
        }

        protected override void BuildUpsertCommand(SqliteCommand command, TCardData card)
        {
            string columns = string.Join(", ", _members.Select(m => $"[{m.ColumnName}]"));
            string parameters = string.Join(", ", _members.Select(m => $"@{m.ColumnName}"));
            command.CommandText = $"INSERT OR REPLACE INTO [{_tableName}] ({columns}) VALUES ({parameters});";

            foreach (var member in _members)
            {
                object raw = member.Get(card);
                object dbValue = ConvertToDb(raw, member.MemberType);
                command.Parameters.AddWithValue($"@{member.ColumnName}", dbValue ?? DBNull.Value);
            }
        }

        /// <summary>
        /// 派生側の例外処理フック。例: EnsureRuntimeDefaults() 呼び出し。
        /// </summary>
        protected virtual void OnEntityLoaded(TCardData entity)
        {
        }

        /// <summary>
        /// 派生側のスキーマ例外処理フック。例: 旧列名から新列名へのデータ移送。
        /// </summary>
        protected virtual void OnSchemaEnsured(SqliteCommand command, HashSet<string> existingColumns)
        {
        }

        protected HashSet<string> GetExistingColumns(SqliteCommand command)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            command.Parameters.Clear();
            command.CommandText = $"PRAGMA table_info([{_tableName}]);";
            using IDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string name = reader["name"]?.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    columns.Add(name);
                }
            }

            return columns;
        }

        private void EnsureTable(SqliteCommand command)
        {
            var primary = _members.FirstOrDefault(m => string.Equals(m.ColumnName, _primaryKeyColumn, StringComparison.OrdinalIgnoreCase));
            if (primary == null)
            {
                throw new InvalidOperationException($"Primary key column '{_primaryKeyColumn}' is not mapped on {typeof(TCardData).Name}.");
            }

            var definitions = new List<string>(_members.Count);
            foreach (var member in _members)
            {
                string definition = string.Equals(member.ColumnName, _primaryKeyColumn, StringComparison.OrdinalIgnoreCase)
                    ? $"[{member.ColumnName}] {member.SqliteType} PRIMARY KEY"
                    : $"[{member.ColumnName}] {member.SqliteType}";
                definitions.Add(definition);
            }

            command.Parameters.Clear();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS [{_tableName}] ({string.Join(", ", definitions)});";
            command.ExecuteNonQuery();
        }

        private IReadOnlyList<MemberMap> BuildMemberMaps()
        {
            var members = new List<MemberMap>();

            foreach (FieldInfo field in typeof(TCardData).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.IsStatic || field.IsInitOnly)
                {
                    continue;
                }

                // 推論できない型は永続化対象から除外する（例: Dictionary など）。
                if (!TryResolveSqliteType(field.FieldType, out string sqliteType))
                {
                    continue;
                }

                members.Add(new MemberMap(
                    field.Name,
                    field.FieldType,
                    sqliteType,
                    target => field.GetValue(target),
                    (target, value) => field.SetValue(target, value)));
            }

            return members;
        }

        private static bool TryResolveSqliteType(Type type, out string sqliteType)
        {
            Type target = Nullable.GetUnderlyingType(type) ?? type;

            if (target == typeof(string))
            {
                sqliteType = "TEXT";
                return true;
            }

            if (target == typeof(int) || target == typeof(long) || target == typeof(short) || target == typeof(byte) || target == typeof(bool))
            {
                sqliteType = "INTEGER";
                return true;
            }

            if (target == typeof(float) || target == typeof(double) || target == typeof(decimal))
            {
                sqliteType = "REAL";
                return true;
            }

            if (target == typeof(List<string>))
            {
                sqliteType = "TEXT";
                return true;
            }

            sqliteType = null;
            return false;
        }

        private static object ConvertToDb(object value, Type memberType)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            Type target = Nullable.GetUnderlyingType(memberType) ?? memberType;

            if (target == typeof(bool))
            {
                return (bool)value ? 1 : 0;
            }

            if (target == typeof(List<string>))
            {
                // 配列型はJSON文字列として保存する。
                return JsonConvert.SerializeObject((List<string>)value);
            }

            return value;
        }

        private static object ConvertFromDb(object value, Type memberType)
        {
            if (value == null || value == DBNull.Value)
            {
                return CreateDefaultValue(memberType);
            }

            Type target = Nullable.GetUnderlyingType(memberType) ?? memberType;
            string text = value.ToString();

            if (target == typeof(string))
            {
                return text;
            }

            if (target == typeof(int))
            {
                return int.TryParse(text, out int parsed) ? parsed : 0;
            }

            if (target == typeof(long))
            {
                return long.TryParse(text, out long parsed) ? parsed : 0L;
            }

            if (target == typeof(short))
            {
                return short.TryParse(text, out short parsed) ? parsed : (short)0;
            }

            if (target == typeof(byte))
            {
                return byte.TryParse(text, out byte parsed) ? parsed : (byte)0;
            }

            if (target == typeof(float))
            {
                return float.TryParse(text, out float parsed) ? parsed : 0f;
            }

            if (target == typeof(double))
            {
                return double.TryParse(text, out double parsed) ? parsed : 0d;
            }

            if (target == typeof(decimal))
            {
                return decimal.TryParse(text, out decimal parsed) ? parsed : 0m;
            }

            if (target == typeof(bool))
            {
                if (int.TryParse(text, out int parsedInt))
                {
                    return parsedInt != 0;
                }

                return bool.TryParse(text, out bool parsedBool) && parsedBool;
            }

            if (target == typeof(List<string>))
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new List<string>();
                }

                try
                {
                    // JSON配列として復元。
                    return JsonConvert.DeserializeObject<List<string>>(text) ?? new List<string>();
                }
                catch (JsonException)
                {
                    // 旧データ互換: 単一文字列が来た場合は1件リスト扱い。
                    return new List<string> { text };
                }
            }

            return CreateDefaultValue(memberType);
        }

        private static object CreateDefaultValue(Type type)
        {
            if (type == typeof(List<string>))
            {
                return new List<string>();
            }

            Type target = Nullable.GetUnderlyingType(type);
            if (target != null)
            {
                return null;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private sealed class MemberMap
        {
            public string ColumnName { get; }
            public Type MemberType { get; }
            public string SqliteType { get; }
            private readonly Func<object, object> _getter;
            private readonly Action<object, object> _setter;

            public MemberMap(string columnName, Type memberType, string sqliteType, Func<object, object> getter, Action<object, object> setter)
            {
                ColumnName = columnName;
                MemberType = memberType;
                SqliteType = sqliteType;
                _getter = getter;
                _setter = setter;
            }

            public object Get(object target) => _getter(target);
            public void Set(object target, object value) => _setter(target, value);
        }
    }
}
