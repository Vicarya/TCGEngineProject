# TCGEngineProject Project Overview

更新日: 2026-03-29

## 1. プロジェクト全体ツリー（フォルダ名ベース）

```text
TCGEngineProject/
|- Assets/
|  |- Scenes/
|  |- Resources/
|  |- StreamingAssets/
|  |  |- WeissSchwarz/
|  |     |- cards.json
|  |     |- cards.db
|  |     |- Decks/
|  |- Scripts/
|  |  |- GameCore/
|  |  |  |- Abilities/
|  |  |  |- Cards/
|  |  |  |- Data/
|  |  |  |- Events/
|  |  |  |- Game/
|  |  |- WeissSchwarz/
|  |     |- Abilities/
|  |     |- Core/
|  |     |- Cost/
|  |     |- Data/
|  |     |  |- Generated/
|  |     |- Definitions/
|  |     |- Effects/
|  |     |- RuleGuides/
|  |     |- Triggers/
|  |     |- UI/
|  |     |- Zones/
|- docs/
|  |- ProjectOverview.md
|  |- ProjectArchitecture.md
|  |- WeissSchwarz.md
|- Packages/
|- ProjectSettings/
|- UserSettings/
|- tools/
|- python/
```

## 2. 機能ごとの分解

### 2.1 UI層（`Assets/Scripts/WeissSchwarz/UI`）

- 役割: 入力受付・表示更新・デッキ編集UI
- 主体クラス:
  - `GameView`
  - `UIGamePlayerController`
  - `DeckEditorManager`
  - 各Zone UIクラス

### 2.2 Weissロジック層（`Assets/Scripts/WeissSchwarz/Core` ほか）

- 役割: Weiss Schwarz 固有のルール、フェーズ、能力、ゾーン挙動
- 主体クラス:
  - `WeissGame`
  - `WeissRuleEngine`
  - `WeissGameState`
  - `StandPhase` ～ `EndPhase`
  - `AbilityFactory` / `CostFactory` / `EffectFactory`

### 2.3 GameCoreロジック層（`Assets/Scripts/GameCore`）

- 役割: タイトル非依存のゲーム基盤
- 主体クラス:
  - `GameBase` / `GameState` / `PhaseBase`
  - `Card` / `CardData` / `Player` / `ZoneBase`
  - `EventBus`
  - `CardRepositoryBase<TCardData>`（DBアクセス抽象基盤）

### 2.4 データ入出力機能（DB）

- インポート専用（Weiss固有）:
  - `WeissCardDbImporter`
  - JSON (`cards.json`) から SQLite (`cards.db`) を生成/更新
- インゲームRuntime（ロード・セーブ）:
  - `CardRepositoryBase<TCardData>`（Core抽象）
  - `WeissCardRuntimeRepository`（Weiss実装）
  - `WeissCardRuntimeStore`（呼び出し窓口）

## 3. 起動時の主要フロー

1. `AppManager.Start()` が `StreamingAssets/WeissSchwarz/cards.db` を `persistentDataPath` にコピー
2. `WeissCardRuntimeStore.Initialize(dbFileName)` で接続先を初期化
3. `WeissCardDatabase.LoadDatabase()` が `WeissCardRuntimeStore.LoadAll()` を呼び出し
4. `GameManager` が `WeissGame` を開始し、UI とルール処理が連携

## 4. DB責務の分離（現行方針）

- Weiss層で持つもの:
  - `WeissCardData` の定義
  - WeissカードのDBスキーマ定義・Readerマッピング定義（`WeissCardRuntimeRepository`）
- Core層で持つもの:
  - DB接続と永続化フローの抽象基盤（`CardRepositoryBase<TCardData>`）
- インポートの扱い:
  - `WeissCardDbImporter` は「JSONをDBへ投入するツール」として限定維持
  - インゲームのロード/セーブ責務は保持しない

