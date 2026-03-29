# Weiss Schwarz モジュールガイド

更新日: 2026-03-29

## 1. データ関連の責務分離

| ファイル | 役割 |
| --- | --- |
| `Assets/Scripts/WeissSchwarz/Data/WeissCardDbImporter.cs` | JSON -> SQLite インポート専用ツール |
| `Assets/Scripts/WeissSchwarz/Data/WeissCardRuntimeStore.cs` | Runtimeロード/セーブの窓口 |
| `Assets/Scripts/WeissSchwarz/Data/WeissCardRuntimeRepository.cs` | Weissカード向けDBマッピング・スキーマ定義 |
| `Assets/Scripts/GameCore/Data/CardRepositoryBase.cs` | DBアクセス共通フロー（抽象基盤） |

## 2. 運用フロー

1. 開発時に `WeissCardDbImporter` で `cards.json` を `cards.db` へ反映
2. 実行時は `AppManager` が DB を `persistentDataPath` へ配置
3. `WeissCardDatabase` / `DeckEditorManager` は `WeissCardRuntimeStore` 経由でデータ参照

## 3. 設計ルール

- Weiss層は「参照データの定義」を担当する
- DBの実操作は `CardRepositoryBase` の抽象フローに乗せる
- 新タイトル追加時は同構造を継承し、最小実装で展開する

