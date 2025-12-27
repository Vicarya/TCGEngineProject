using UnityEngine;
using TCG.Core;
using System;
using System.Collections.Generic;

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

        // --- 互換性維持・エイリアスのためのプロパティ ---
        // 以下のプロパティは、古いコードや、異なる命名規則を持つ外部データソース（JSONなど）との
        // 後方互換性を維持するために定義されたエイリアス（別名）です。
        // 例えば、`card_no`へのアクセスは、内部的には基底クラスの`CardCode`プロパティにリダイレクトされます。
        // また、一部のプロパティはデータバインディングの容易さのため、int型とstring型で相互変換を行っています。

        public string card_no { get => CardCode; set => CardCode = value; }
        public string name { get => Name; set => Name = value; }
        public string detail_page_url { get; set; }
        public string image_url { get => ImagePath; set => ImagePath = value; }

        // Japanese aliases (C# allows Unicode identifiers)
        public string サイド { get => Side; set => Side = value; }
        public string 種類 { get => CardType; set => CardType = value; }

        // Numeric fields exposed as strings for compatibility with importer/UI
        public string レベル { get => Level.ToString(); set => Level = int.TryParse(value, out var v) ? v : 0; }
        public string 色 { get => Color; set => Color = value; }
        public string パワー { get => Power.ToString(); set => Power = int.TryParse(value, out var v) ? v : 0; }
        public string ソウル { get => Soul.ToString(); set => Soul = int.TryParse(value, out var v) ? v : 0; }
        public string コスト { get => Cost.ToString(); set => Cost = int.TryParse(value, out var v) ? v : 0; }
        public string レアリティ { get => Rarity; set => Rarity = value; }
        public string トリガー { get => TriggerIcon; set => TriggerIcon = value; }
        public List<string> 特徴 { get => Traits; set => Traits = value; }
        public string flavor_text { get => FlavorText; set => FlavorText = value; }
        public List<string> abilities { get => Abilities; set => Abilities = value; }
    }
}