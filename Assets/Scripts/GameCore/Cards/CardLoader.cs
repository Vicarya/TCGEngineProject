using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Core
{
    /// <summary>
    /// Resourcesフォルダ内のScriptableObjectアセットから全てのカードデータをロードします。
    /// 【注意】現状の実装は `WeissCardData` に直接依存しており、GameCoreの汎用性が損なわれています。
    /// 将来的には、ジェネリックなデータローダーにリファクタリングすることが望ましいです。
    /// </summary>
    public static class CardLoader
    {
        /// <summary>
        /// ロードされた全てのヴァイスシュヴァルツのカードデータのリスト。
        /// </summary>
        public static List<WeissCardData> AllCards { get; private set; } = new List<WeissCardData>();

        /// <summary>
        /// カードデータアセットが格納されているResourcesフォルダ内のパス。
        /// </summary>
        private const string CardDataPath = "CardData";

        /// <summary>
        /// Resources/CardData フォルダから全ての `WeissCardDataAsset` ファイルをロードし、
        /// `AllCards` リストを初期化します。
        /// </summary>
        public static void LoadAllCardAssets()
        {
            // Resources.LoadAll を使って、指定されたパスにある全てのWeissCardDataAssetを読み込む
            var cardAssets = Resources.LoadAll<WeissCardDataAsset>(CardDataPath);

            // 読み込んだアセットから実際のCardDataを取り出し、リストに変換する
            AllCards = cardAssets.Select(asset => asset.Data).ToList();

            Debug.Log($"ヴァイスシュヴァルツのカードを {AllCards.Count} 枚ロードしました。(from: Resources/{CardDataPath})");
        }
    }
}