using TCG.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Weiss {
    /// <summary>
    /// ヴァイスシュヴァルツカードの永続的な（ゲーム中に変化しない）属性を保持するデータコンテナクラス。
    /// [System.Serializable]属性により、JSONからのデシリアライズやUnityエディタでの表示が可能です。
    /// このデータは、ゲーム中のカード実体であるWeissCardインスタンスに供給されます。
    /// </summary>
    [System.Serializable]
    public class WeissCardData : CardData
    {
        // --- 基本プロパティ ---

        /// <summary>キャラクターのレベル。</summary>
        public int Level;
        /// <summary>カードをプレイするためのコスト。</summary>
        public int Cost;
        /// <summary>キャラクターの戦闘力。</summary>
        public int Power;
        /// <summary>アタック時に与えるダメージの基準値。</summary>
        public int Soul;
        /// <summary>カードのサイド（ヴァイスサイドかシュヴァルツサイドか）。</summary>
        public string Side;
        /// <summary>カードの色（黄、緑、赤、青）。</summary>
        public string Color;
        /// <summary>カードの種類（キャラクター、イベント、クライマックス）。</summary>
        public string CardType;
        /// <summary>カード上部にあるトリガーアイコンの種類。</summary>
        public string TriggerIcon;
        /// <summary>カードに書かれているフレーバーテキスト。</summary>
        public string FlavorText;
        /// <summary>カードが持つ能力のテキスト原文のリスト。</summary>
        public List<string> Abilities;
        /// <summary>カードが持つ特徴（例：「音楽」「武器」など）のリスト。</summary>
        public List<string> Traits;
        /// <summary>公式カード詳細ページのURL。</summary>
        public string DetailPageUrl;

        /// <summary>
        /// ランタイムで必要な既定値や派生値を補完します。
        /// </summary>
        public void EnsureRuntimeDefaults()
        {
            Abilities ??= new List<string>();
            Traits ??= new List<string>();

            if (string.IsNullOrEmpty(WorkId) && !string.IsNullOrEmpty(CardCode))
            {
                int separatorIndex = CardCode.IndexOf('/');
                WorkId = separatorIndex > 0 ? CardCode.Substring(0, separatorIndex) : CardCode;
            }
        }

        /// <summary>
        /// カードが持つ能力テキストを常にnullではないコレクションとして返します。
        /// </summary>
        public IEnumerable<string> GetAbilityTexts()
        {
            return Abilities ?? Enumerable.Empty<string>();
        }
    }
}
