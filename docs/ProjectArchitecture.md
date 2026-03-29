# Project Architecture

更新日: 2026-03-29

## 1. 設計思想

- `TCG.Core`:
  - タイトル非依存の汎用ロジックを配置
  - ルールの共通土台、イベント、汎用データアクセス抽象を担当
- `TCG.Weiss`:
  - Weiss Schwarz 専用ロジックを配置
  - Coreを継承/実装してゲーム固有挙動を提供

この分離により、他TCGを追加する際は `TCG.Core` を再利用し、`TCG.<GameName>` 側を差し替える構造を維持する。

## 2. 層構成（依存方向）

```text
UI層 (TCG.Weiss.UI)
  -> Weissロジック層 (TCG.Weiss.*)
    -> GameCoreロジック層 (TCG.Core.*)
```

依存ルール:
- `TCG.Core` は `TCG.Weiss` を参照しない
- UIは `TCG.Weiss` 経由でゲーム進行を操作する
- DB実操作は Core抽象基盤 + Weiss実装に集約する

## 3. 継承・実装の主要構造

### 3.1 ゲーム進行

```text
GameBase (TCG.Core)
  └─ WeissGame (TCG.Weiss)

GameState (TCG.Core)
  └─ WeissGameState (TCG.Weiss)

PhaseBase (TCG.Core)
  ├─ SimplePhase (TCG.Weiss)
  ├─ StandPhase
  ├─ DrawPhase
  ├─ ClockPhase
  ├─ MainPhase
  ├─ ClimaxPhase
  ├─ AttackPhase
  └─ EndPhase
```

### 3.2 カード/クエリ

```text
CardData (TCG.Core)
  └─ WeissCardData (TCG.Weiss)

CardQuery<TCardData> (TCG.Core)
  └─ WeissCardQuery (TCG.Weiss)

CardDatabase<TCardData, TQuery> (TCG.Core)
  └─ WeissCardDatabase (TCG.Weiss)
```

### 3.3 DBアクセス（今回の方針）

```text
CardRepositoryBase<TCardData> (TCG.Core, abstract)
  └─ WeissCardRuntimeRepository (TCG.Weiss.Data)
       ├─ SELECT定義
       ├─ Reader -> WeissCardData マッピング
       ├─ スキーマ定義
       └─ UPSERT定義

WeissCardRuntimeStore (TCG.Weiss.Data)
  ├─ 初期化
  ├─ LoadAll()
  └─ SaveAll()

WeissCardDbImporter (TCG.Weiss.Data)
  └─ JSON -> DB インポート専用（ツール責務）
```

責務ルール:
- Weiss層は「どのデータを参照するか（スキーマ・マッピング）」を定義する
- 実際の保存処理フローは `CardRepositoryBase<TCardData>` に集約する
- インゲームロード/セーブは `WeissCardRuntimeStore` 経由に統一する
- `WeissCardDbImporter` はインポート用途に限定する

## 4. 名前空間命名規則

## 4.1 基本ルール

- ルートは `TCG` で統一する
- 汎用ロジックは `TCG.Core` 配下
- タイトル固有ロジックは `TCG.<GameName>` 配下
- `GameName` は PascalCase（例: `Weiss`, `Pokemon`）

## 4.2 推奨構造

```text
TCG.Core
TCG.Core.Game
TCG.Core.Events
TCG.Core.Data

TCG.Weiss
TCG.Weiss.UI
TCG.Weiss.Data
TCG.Weiss.Effects
TCG.Weiss.Definitions
```

## 4.3 禁止/非推奨

- `TCG.Core` から `TCG.Weiss` を参照すること
- `TCG.<GameA>` から `TCG.<GameB>` を直接参照すること
- Unity依存(`MonoBehaviour` など)を `TCG.Core` に持ち込むこと

## 5. 他TCGへ展開する設計方針

1. `TCG.Core` は変更最小で再利用する
2. 新タイトル用に `TCG.<NewGame>` 名前空間を追加する
3. `CardData` / `GameState` / `GameBase` / `Query` / `Database` 派生を実装する
4. DBは以下の分離を守る
   - Core: `CardRepositoryBase<TCardData>` の抽象フロー
   - Game固有: `<NewGame>RuntimeRepository` でスキーマ/マッピング定義
   - ツール: `<NewGame>DbImporter` でインポートのみ実装
5. UIは `I<NewGame>PlayerController` などの境界インターフェース経由で接続する

## 6. 現在構成の確認結果（実装ベース）

- `TCG.Core -> TCG.Weiss` の逆参照は無し
- DB実操作は `CardRepositoryBase` + `WeissCardRuntimeRepository` に集約済み
- `AppManager`, `WeissCardDatabase`, `DeckEditorManager` は `WeissCardRuntimeStore` を利用
- `CardDataImporter` は `WeissCardDbImporter` へ改名し、インポート専用責務に整理済み

