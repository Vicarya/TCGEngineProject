using System.Collections.Generic;
using Mono.Data.Sqlite;
using TCG.Core;

namespace TCG.Weiss.Data
{
    /// <summary>
    /// WeissカードのRuntimeロード/セーブ実装。
    /// 通常処理はAutoSqliteCardRepositoryBaseに委譲し、
    /// Weiss固有の例外(旧カラム移行/ロード後補正)のみ実装する。
    /// </summary>
    public sealed class WeissCardRuntimeRepository : AutoSqliteCardRepositoryBase<WeissCardData>
    {
        public WeissCardRuntimeRepository(string connectionString)
            : base(connectionString, "cards", "CardCode")
        {
        }

        protected override void OnEntityLoaded(WeissCardData entity)
        {
            // DBに不足情報があっても、Runtimeで必要な既定値を補完する。
            entity.EnsureRuntimeDefaults();
        }

        protected override void OnSchemaEnsured(SqliteCommand command, HashSet<string> existingColumns)
        {
            MigrateLegacyColumns(command, existingColumns);
        }

        private static void MigrateLegacyColumns(SqliteCommand command, HashSet<string> existingColumns)
        {
            // 旧スキーマ(スネークケース列) -> 新スキーマ(モデル名列) の移行マップ。
            var mappings = new (string NewColumn, string OldColumn)[]
            {
                ("CardCode", "card_no"),
                ("Name", "name"),
                ("WorkId", "work_id"),
                ("DetailPageUrl", "detail_page_url"),
                ("ImagePath", "image_url"),
                ("Side", "side"),
                ("CardType", "type"),
                ("Level", "level"),
                ("Cost", "cost"),
                ("Power", "power"),
                ("Soul", "soul"),
                ("Color", "color"),
                ("Rarity", "rarity"),
                ("TriggerIcon", "trigger"),
                ("Traits", "features"),
                ("FlavorText", "flavor_text"),
                ("Abilities", "abilities")
            };

            foreach (var (newColumn, oldColumn) in mappings)
            {
                if (!existingColumns.Contains(oldColumn))
                {
                    continue;
                }

                // 新列が空の場合のみ旧列の値をコピーする。
                command.Parameters.Clear();
                command.CommandText = $@"
                    UPDATE [cards]
                    SET [{newColumn}] = COALESCE([{newColumn}], [{oldColumn}])
                    WHERE [{newColumn}] IS NULL
                      AND [{oldColumn}] IS NOT NULL;";
                command.ExecuteNonQuery();
            }
        }
    }
}
