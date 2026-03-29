using System.Collections.Generic;
using TCG.Core;
using TCG.Weiss.Data;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツのカードデータに特化したデータベース。
    /// 汎用的な CardDatabase を継承し、具体的なデータロード処理を実装しています。
    /// シングルトンとして設計されており、ゲーム内のどこからでもアクセス可能です。
    /// </summary>
    public class WeissCardDatabase : CardDatabase<WeissCardData, WeissCardQuery>
    {
        /// <summary>
        /// データベースのシングルトンインスタンス。
        /// </summary>
        public static WeissCardDatabase Instance { get; private set; }

        /// <summary>
        /// ロードされたすべてのカードデータへの読み取り専用アクセス。
        /// </summary>
        public IReadOnlyList<WeissCardData> AllCards => allCards;

        /// <summary>
        /// オブジェクト初期化時に呼び出され、シングルトンインスタンスを設定し、データベースをロードします。
        /// </summary>
        private void Awake()
        {
            Instance = this;
            LoadDatabase();
        }

        /// <summary>
        /// データベースにすべてのカードデータをロードします。
        /// </summary>
        public override void LoadDatabase()
        {
            allCards = WeissCardRuntimeStore.LoadAll();

            if (allCards.Count > 0)
            {
                Debug.Log($"WeissCardDatabase: {allCards.Count} cards loaded.");
            }
            else
            {
                Debug.LogWarning("WeissCardDatabase: no cards were loaded.");
            }
        }
    }
}
