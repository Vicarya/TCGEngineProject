# TCGEngineProject Project Overview

## 1. ドキュメントの目的
このドキュメントは、`TCGEngineProject` の現在の実装を対象に、以下を一元整理したものです。

- フォルダ名ベースのプロジェクト全体構成
- モジュール境界と責務
- 主要機能の詳細設計（アーキテクチャ）

対象時点: 2026-03-29

---

## 2. プロジェクトツリー（フォルダ名ベース）

### 2.1 ルート構成

```text
TCGEngineProject/
|- Assets/
|- docs/
|- Packages/
|- ProjectSettings/
|- UserSettings/
|- tools/                 
|- python/                (Pythonツール) 
|- .vscode/
|- Library/               (Unity生成物)
|- Temp/                  (Unity生成物)
|- Logs/                  (Unity生成物)
|- obj/                   (ビルド生成物)
|- tmp_nuget/             (一時生成物)
|- *.csproj / *.sln
```

### 2.2 Assets 配下

```text
Assets/
|- Fonts/
|- Plugins/
|  |- SQLite/
|  |- x86_64/
|- Resources/
|  |- Images/
|  |- Prefabs/
|- Scenes/
|- Scripts/
|  |- GameCore/
|  |  |- Abilities/
|  |  |- Cards/
|  |  |- Data/
|  |  |- Events/
|  |  |- Game/
|  |- WeissSchwarz/
|  |  |- Abilities/
|  |  |  |- Core/
|  |  |- Core/
|  |  |- Cost/
|  |  |- Data/
|  |  |  |- Generated/
|  |  |- Definitions/
|  |  |- Effects/
|  |  |- RuleGuides/
|  |  |- Triggers/
|  |  |- UI/
|  |  |- Zones/
|  |- TCGPokemon/
|     |- Zone/
|- StreamingAssets/
|  |- WeissSchwarz/
|     |- Decks/
|- TextMesh Pro/
```

### 2.3 docs 配下

```text
docs/
|- ProjectOverview.md
|- ProjectArchitecture.md
|- TODO_ActionPlan.md
|- TODO_Handover.md
|- diagrams/
```

### 2.4 補助/環境系ディレクトリ

- `Packages/`: Unity Package Manager 定義
- `ProjectSettings/`: Unityプロジェクト設定
- `UserSettings/`: ローカル環境依存設定
- `python/`, `tools/`: データ作成・環境構築用の補助スクリプト群。詳細は 5.7 参照。

---

## 3. 構成整理（責務別）

### 3.1 `TCG.Core`（共通エンジン層）
`Assets/Scripts/GameCore`

責務:
- ゲーム進行の抽象 (`GameBase`, `GameState`, `PhaseBase`)
- カード/ゾーン/プレイヤーの共通モデル (`Card`, `CardData`, `Player`, `IZone`, `ZoneBase`)
- 能力解決の共通契約 (`AbilityBase`, `IEffect`, `ITriggerCondition`, `ICost`)
- イベント通知 (`EventBus`, `GameEventType`, `BaseGameEvents`)
- データ読み込み基盤 (`CardLoader`, `CardDatabase`, `CardQuery`)

### 3.2 `TCG.Weiss`（Weiss Schwarz ルール実装層）
`Assets/Scripts/WeissSchwarz`

責務:
- Weiss固有のゲーム状態/ターン進行 (`WeissGame`, `WeissGameState`, `WeissPhaseFactory`, 各Phase)
- ルール解決エンジン (`WeissRuleEngine`)
- ゾーン具体実装 (`DeckZone`, `StageZone`, `ClockZone`, `StockZone` など)
- 能力/コスト/効果の解析と実体化 (`AbilityFactory`, `CostFactory`, `EffectFactory`)
- データアクセス (`CardDataImporter`, `WeissCardDatabase`, `WeissCardQuery`)
- UI連携 (`GameView`, `UIGamePlayerController`, 各Zone UI, `DeckEditorManager`)

### 3.3 `TCGPokemon`（将来拡張/試作層）
`Assets/Scripts/TCGPokemon`

責務:
- 別TCG対応のための試作コード配置
- 現状はコメントアウト主体で、本流機能ではない

---

## 4. アーキテクチャ概要

### 4.1 レイヤ構成

```text
[Presentation]
  GameView / Zone UI / DeckEditor / UIGamePlayerController
      |
[Application/Game Specific]
  WeissGame / WeissPhases / WeissRuleEngine / AbilityQueue
      |
[Domain Core]
  GameBase / GameState / PhaseBase / Card / Zone / Ability / EventBus
      |
[Infrastructure]
  AppManager / CardDataImporter / CardLoader / SQLite / StreamingAssets
```

### 4.2 依存方向

- `GameCore` は `WeissSchwarz` を参照しない（共通基盤）
- `WeissSchwarz` は `GameCore` を参照して具体化する
- UI は `WeissSchwarz.Core` の状態を読んで描画し、`IWeissPlayerController` 経由で入力を返す
- データ層は SQLite/JSON を `WeissCardData` に正規化して供給する

---

## 5. 主要機能の詳細設計（アーキテクチャ）

## 5.1 起動・データ初期化フロー

### 目的
ゲーム開始前に、カードDBとUIが参照可能な初期状態を作る。

### 主要コンポーネント
- `AppManager`
- `CardDataImporter`
- `WeissCardDatabase`
- `GameManager`

### 処理フロー
1. `AppManager.Start()` で `CardDataImporter.Initialize(dbFileName)` を実行
2. `StreamingAssets/WeissSchwarz/cards.db` を `persistentDataPath` へコピー
3. 完了後 `OnDataInitialized` を発火
4. `WeissCardDatabase.Awake()` で `cards` テーブルを読み込みメモリ化
5. `GameManager.Start()` で `WeissGame` を生成し、マリガン→ゲーム開始

### 設計上の要点
- DBコピー失敗時は stale DB を削除し、不整合を回避
- エディタでは `CardDataImporter.GenerateDatabaseInEditor` で JSON から DB 再生成可能

---

## 5.2 ターン進行（Phase駆動）

### 目的
Weiss Schwarz の 1ターン進行を統一ルールで実行する。

### 主要コンポーネント
- `WeissPhaseFactory`
- `PhaseBase` と各フェーズ実装
- `WeissRuleEngine.ExecuteTurn()`

### フェーズ列
- `StandPhase`
- `DrawPhase`
- `ClockPhase`
- `MainPhase`
- `ClimaxPhase`
- `AttackPhase`
- `EndPhase`

### 処理フロー
1. `WeissRuleEngine` がターンフェーズツリーを構築
2. `TurnPhaseTree.Execute(GameState)` で親→子を順次実行
3. 各フェーズの `OnEnter` でルール処理と `GameEvent` 通知を行う
4. `CheckTiming` 系イベントで誘発チェックタイミングを統一

### 設計上の要点
- 進行制御を `PhaseBase` に集約し、ルール追加点をフェーズ単位に限定
- フェーズ横断通知は `EventBus` で疎結合化

---

## 5.3 行動解決（プレイ/攻撃/ダメージ/アンコール）

### 目的
メイン行動と戦闘解決を段階的に実施し、状態遷移を明確化する。

### 主要コンポーネント
- `MainPhase`
- `AttackPhase`
- `WeissRuleEngine` (`TriggerCheck`, `ApplyDamage`, `ResolveCounterAbility`)
- `StageZone`, `ClockZone`, `LevelZone`, `WaitingRoomZone`

### MainPhase（抜粋）
- `MainPhaseAction` をコントローラから取得
- カードプレイ時に レベル/コスト/配置可否 を検証
- コスト支払い後に `CardPlayed` イベントを発行
- 起動能力は `ActivateAbility` に委譲

### AttackPhase（抜粋）
- 攻撃宣言→トリガー→カウンター→ダメージ→バトル→アンコールの順
- `AttackType`（Direct/Front/Side）で分岐
- ダメージキャンセル/レベルアップ/リフレッシュダメージを `WeissRuleEngine` で処理

### 設計上の要点
- 戦闘解決をステップ化し、ログ/イベントと一緒に追跡可能
- 盤面管理（`StageZone` + `StageSlot`）とリソース管理（`Stock/Clock/Level`）を明確に分離

---

## 5.4 能力システム（文字列定義→実行）

### 目的
カードテキスト由来の能力を実行可能オブジェクトへ変換し、ルールエンジンで解決する。

### 主要コンポーネント
- `AbilityFactory`
- `CostFactory`
- `EffectFactory`
- `WeissAbility` / `AbilityBase`
- `AbilityQueue` / `PendingAbility`

### 処理フロー
1. `WeissCard` 生成時に `AbilityFactory.CreateAbilitiesForCard` を実行
2. 能力文字列を `AbilityType` / `Cost` / `Effect` に分解
3. 誘発時は `WeissRuleEngine.CheckForTriggeredAbilities` が `PendingAbility` を積む
4. `ResolveAbilityQueue` でプレイヤー順に解決対象を決定し実行

### 設計上の要点
- 文字列解釈は Factory に閉じ込め、ルール進行から分離
- 未対応テキストは警告出力に倒し、段階的拡張を許容
- `AbilityQueue` で同時誘発の順序選択に対応

---

## 5.5 UI制御とプレイヤー入力

### 目的
ゲームロジックとUI入力をインターフェースで分離し、差し替え可能にする。

### 主要コンポーネント
- `IWeissPlayerController`
- `ConsolePlayerController`
- `UIGamePlayerController`
- `GameView`

### 設計方針
- ルール側は `IWeissPlayerController` のみ参照
- UI実装は `TaskCompletionSource` で非同期入力を返却（マリガン等）
- 表示更新は `GameView.UpdateView(player)` を入口に各Zone UIへ配信

### 現状の実装状態
- `UIGamePlayerController` は未実装メソッドが多く、現時点はダミー返却が混在
- `ConsolePlayerController` の併用を前提とした過渡状態

---

## 5.6 デッキ編集機能

### 目的
カード検索・フィルタ・ページング・デッキ編集をUI上で完結させる。

### 主要コンポーネント
- `DeckEditorManager`
- `WeissCardQuery`
- `PaginationUI`

### 処理フロー
1. `AppManager.OnDataInitialized` を受けて全カードをロード
2. 検索条件を `WeissCardQuery` に積み上げて絞り込み
3. ページ単位でカードグリッド表示
4. デッキ枚数制約（50枚/同名4枚）を満たす範囲で編集

### 設計上の要点
- フィルタ条件を Query オブジェクトに集約
- UIイベントを `DeckEditorManager` に集約し、画面ロジックを一本化

---

## 5.7 補助ツール（Python/Tools）

### 目的
カードデータの収集、画像リソースの管理、環境構築の自動化など、Unityエディタ外での作業を効率化する。

### 主な役割
- **データ管理 (`python/`)**:
    - **スクレイピング**: 外部ソースからカード情報を取得し、`cards.json` 形式で出力。
    - **バリデーション**: JSONデータの整合性（ID重複やフォーマット）をチェック。
    - **画像処理**: カード画像のダウンロードや、ゲーム内利用に最適なサイズへの一括リサイズ。
- **環境構築 (`tools/`)**:
    - プロジェクトセットアップ用のシェルスクリプトや、ビルド補助ツールの配置。

### 具体的なツールと実行方法

| ディレクトリ | ファイル名 | 実行方法 (例) | 説明 |
| :--- | :--- | :--- | :--- |
| `python/` | `card_scraper.py` | `python python/card_scraper.py --all` | 公式サイト等からカード情報を取得し `cards.json` を生成。 |
| `python/` | `json_validator.py` | `python python/json_validator.py` | `cards.json` の構文やIDの重複、必須項目の欠落をチェック。 |
| `python/` | `image_resizer.py` | `python python/image_resizer.py` | 取得したカード画像をUnityに最適なサイズ・形式（WebP/PNG）に一括変換。 |
| `tools/` | `setup_env.sh` | `bash tools/setup_env.sh` | 開発環境（Git Hookや依存ライブラリ）の初期セットアップ。 |
| `tools/` | `build_assets.py` | `python tools/build_assets.py` | アセットバンドルのビルドや、デプロイ用データのパッケージング補助。 |

### 実行環境の注意
- Python 3.10以上を推奨。
- `python/requirements.txt` が存在する場合は、事前に `pip install -r python/requirements.txt` を実行してください。
- Unityエディタ上からこれらのツールを呼び出すメニュー（`Tools > Weiss Schwarz > ...`）も順次実装予定です。

---

## 6. 主要モジュール間シーケンス（要約）

```text
AppManager -> CardDataImporter -> SQLite準備
        -> WeissCardDatabase -> メモリロード
GameManager -> WeissGame -> WeissRuleEngine
         -> (Phase実行)
         -> EventBus Raise
         -> RuleEngine (誘発検出/解決)
         -> UI Controller (選択入力)
         -> GameView (表示更新)
```

---

## 7. 今後の設計上の優先課題

- `UIGamePlayerController` の未実装入力を実装し、Console依存を解消
- 能力テキスト解析の対応範囲拡大（`CostFactory`/`EffectFactory`）
- `WeissRuleEngine` のハードコード能力分岐をDefinition駆動へ移行
- `ProjectArchitecture.md` との内容重複を解消し、設計書を一本化
- 文字コードをUTF-8へ統一（既存ドキュメントの文字化け対策）
