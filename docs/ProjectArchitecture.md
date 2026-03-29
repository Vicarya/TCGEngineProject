# Project Architecture

## 1. 目的
本ドキュメントは、`TCG.Core` を汎用ロジック、`TCG.Weiss` を Weiss Schwarz 専用ロジックとして分離する設計思想を明文化し、継承関係・依存関係・拡張方針を整理する。

対象時点: 2026-03-29

---

## 2. 設計思想（Namespace境界）

### 2.1 `TCG.Core` の責務
`TCG.Core` は「カードゲーム一般」に共通する抽象を持つ。

- ゲーム進行の抽象: `GameBase`, `GameState`, `PhaseBase`
- プレイヤー/カード/ゾーンの抽象: `Player`, `Card`, `CardData`, `IZone<T>`, `ZoneBase<T>`
- 能力解決の抽象: `AbilityBase`, `ICost`, `IEffect`, `ITriggerCondition`
- イベント基盤: `EventBus`, `GameEvent`, `GameEventType`
- データアクセス抽象: `CardLoader`, `CardDatabase<TCardData, TQuery>`, `CardQuery<TCardData>`

### 2.2 `TCG.Weiss` の責務
`TCG.Weiss` は Weiss Schwarz 固有ルールを `TCG.Core` の抽象へマッピングする。

- 固有ゲーム本体: `WeissGame`, `WeissGameState`, `WeissRuleEngine`
- 固有フェーズ: `StandPhase`〜`EndPhase`
- 固有カード/クエリ/DB: `WeissCard`, `WeissCardData`, `WeissCardQuery`, `WeissCardDatabase`
- 固有ゾーン群: `DeckZone`, `StageZone`, `ClockZone`, `StockZone` など
- 固有能力解析: `AbilityFactory`, `CostFactory`, `EffectFactory`
- UIアダプタ: `IWeissPlayerController`, `ConsolePlayerController`, `UIGamePlayerController`

### 2.3 依存ルール

- `TCG.Core` -> `TCG.Weiss` の参照は禁止
- `TCG.Weiss` -> `TCG.Core` の参照は許可
- UI層（Unity `MonoBehaviour`）は `TCG.Weiss` の外側アダプタとして扱う
- ルール本体は `MonoBehaviour` に依存しない

### 2.4 名前空間の命名規則

基本方針:
- ルートは `TCG` で統一する
- 共通基盤は `TCG.Core` に集約する
- タイトル固有は `TCG.<GameName>` を使う（例: `TCG.Weiss`）

推奨パターン:
- `TCG.Core`: 汎用ドメインモデル/抽象
- `TCG.Core.<Area>`: Core内の責務分割（必要時のみ）
- `TCG.<GameName>`: ゲーム固有のドメイン実装
- `TCG.<GameName>.UI`: ゲーム固有UI層
- `TCG.<GameName>.Data`: ゲーム固有データアクセス層
- `TCG.<GameName>.<Area>`: `Core`, `Zones`, `Effects`, `Cost` など責務単位

`GameName` の命名:
- PascalCase を使用（例: `Weiss`, `Pokemon`, `Yugioh`）
- 省略語は可読性優先（チームで表記を固定）

禁止事項:
- `TCG.Core` にゲーム名を含む名前空間/型を置かない
- `TCG.<GameName>` から別ゲーム名前空間（`TCG.OtherGame`）を参照しない
- `Unity` 依存 (`MonoBehaviour`, `SerializeField`) を Core層に持ち込まない

ファイル配置との対応:
- `Assets/Scripts/GameCore/**` -> `namespace TCG.Core`（または `TCG.Core.*`）
- `Assets/Scripts/WeissSchwarz/**` -> `namespace TCG.Weiss`（または `TCG.Weiss.*`）

命名例:
- 良い例: `TCG.Core`, `TCG.Weiss`, `TCG.Weiss.UI`, `TCG.Weiss.Data`
- 避ける例: `TCG.GameCore`, `TCG.CommonWeiss`, `GameCore`, `WeissSchwarz`（ルート無し）

---

## 3. 継承・実装構造

## 3.1 Core 抽象の継承軸

```text
CardData
  ^
  +-- WeissCardData

Card
  ^
  +-- CardBase<TData>
        ^
        +-- WeissCard

Player
  ^
  +-- WeissPlayer

GameBase
  ^
  +-- WeissGame

GameState
  ^
  +-- WeissGameState

PhaseBase
  ^
  +-- SimplePhase
  +-- StandPhase / DrawPhase / ClockPhase / MainPhase / ClimaxPhase / AttackPhase / EndPhase
```

## 3.2 Zone 抽象の継承軸

```text
IZone
  ^
  +-- IZone<TCard>

IZone<TCard>
  ^
  +-- ZoneBase<TCard>
        ^
        +-- DeckZoneBase<TCard>
              ^
              +-- DeckZone (Weiss)
        +-- WeissZone
              +-- HandZone / WaitingRoomZone / StageZone / StageSlot
              +-- ClockZone / LevelZone / StockZone / ClimaxZone
              +-- MemoryZone / ResolutionZone / MarkerZone
```

## 3.3 Ability 抽象の継承軸

```text
AbilityBase
  ^
  +-- WeissAbility

IEffect
  +-- LookTopAndPlaceEffect / PowerBoostEffect / SoulBoostEffect / ...

ICost
  +-- StockCost<TCard> / ClockCost<TCard> / DiscardCost / RestCost / ...
```

## 3.4 Query/Database 抽象の継承軸

```text
CardQuery<TCardData>
  ^
  +-- WeissCardQuery

CardDatabase<TCardData, TQuery>
  ^
  +-- WeissCardDatabase
```

注記:
- `TCG.Core` 内に `CardQuery<TCard, TQuery>`（CRTP版）も存在し、現状は `WeissCardQuery` が `CardQuery<TCardData>` 系を利用している。
- 別TCG展開前に Query 基底の一本化方針を決めると保守性が上がる。

---

## 4. レイヤ設計

```text
[Presentation]
  GameView, ZoneUI, DeckEditorManager, UIGamePlayerController

[Game Specific Domain: TCG.Weiss]
  WeissGame, WeissRuleEngine, WeissPhases, WeissZones, WeissAbility model

[Core Domain: TCG.Core]
  GameBase/GameState/PhaseBase, Card/Zone, Ability contracts, EventBus

[Infrastructure]
  AppManager, CardDataImporter, SQLite(CardLoader), StreamingAssets
```

### 4.1 レイヤ間の接続

- ルール処理のエントリは `WeissGame` / `WeissRuleEngine`
- 入力は `IWeissPlayerController` 経由で注入（Console/UI差し替え）
- フェーズ内イベントは `EventBus` で通知し、直接依存を減らす
- カード定義データは `CardDataImporter` -> `WeissCardDatabase` で供給

---

## 5. 現在の設計評価

### 5.1 良い点

- CoreとWeissの責務分離は明確
- Card/Zone/Phase/Ability の抽象軸が再利用向き
- Player Controller をインターフェース化し、UI依存を隔離
- EventBus によりフェーズ横断処理を疎結合化

### 5.2 改善ポイント

- `WeissRuleEngine` に能力文字列のハードコード分岐が残っている
- `UIGamePlayerController` は未実装メソッドが多く、実行経路が暫定
- `CardQuery` 基底が重複しており、拡張時に方針がぶれやすい

---

## 6. 別TCGへ展開する際の設計方針

## 6.1 原則

- 共通化できるものは `TCG.Core` へ
- ルール固有は `TCG.<GameName>` へ
- UIはゲームロジックから切り離し、`I<GameName>PlayerController` で橋渡し
- 文字列直解釈より Definition/Factory 駆動を優先

## 6.2 新規TCG実装の推奨構成

```text
Assets/Scripts/<GameName>/
|- Core/
|  |- <GameName>Game : GameBase
|  |- <GameName>GameState : GameState
|  |- <GameName>RuleEngine
|  |- <GameName>Player : Player
|  |- <GameName>Phases
|  |- I<GameName>PlayerController
|- Cards/
|  |- <GameName>CardData : CardData
|  |- <GameName>Card : CardBase<<GameName>CardData>
|  |- <GameName>CardQuery : CardQuery<<GameName>CardData>
|  |- <GameName>CardDatabase : CardDatabase<...>
|- Zones/
|  |- 固有Zone実装
|- Abilities/
|  |- AbilityFactory, Definition
|- Cost/
|- Effects/
|- UI/
```

## 6.3 移植時の実装順序

1. `CardData` / `Card` / `Query` / `Database` を定義
2. `Player` と固有Zoneを作成
3. フェーズを最小セットで実装（ドロー、メイン、終了など）
4. `RuleEngine` で勝敗/ダメージ/固有処理を実装
5. `I<GameName>PlayerController` の Console 実装で先に動作確認
6. 最後に UI 実装を接続

## 6.4 Coreへ昇格する判断基準

以下を満たす場合は `TCG.Core` へ移す。

- 2タイトル以上で同じ責務/データ構造が必要
- ルール固有の語彙を含まず抽象化できる
- API変更が既存タイトルへ破壊的影響を与えない

---

## 7. 実装ルール（運用ガイド）

- `TCG.Core` にゲーム名（Weiss/Pokemon等）を含む型名を置かない
- `TCG.<GameName>` から `TCG.Core` の抽象を継承/実装する
- `MonoBehaviour` はアプリケーション境界（起動/表示/入力）でのみ使用する
- フェーズ処理は `PhaseBase` を起点にし、フェーズ外に重複ロジックを散らさない
- 能力・コスト・効果はFactory拡張で増やし、RuleEngineへの直書きを減らす

---

## 8. まとめ

本プロジェクトの設計思想は「Coreに抽象、Gameに具体」を正しく採用できている。今後の別TCG展開では、

- Core抽象の安定化
- 専用ルールのFactory/Definition化
- UI実装の完成

を優先すると、再利用性と保守性を同時に高められる。
