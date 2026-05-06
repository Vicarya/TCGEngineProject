# TCG Engine - Action Plan

## 1. ドキュメントの目的と運用ルール

このドキュメントは、本TCGエンジンを「ヴァイスシュヴァルツの対戦シミュレーションが可能なゲーム」として完成させるための開発ロードマップです。

### 運用ルール
- **担当の明確化**: タスクに着手する際は、担当者欄に自分の名前を記載してください。
- **ステータスの更新**: タスクの進捗に応じて、ステータスを `未着手` -> `進行中` -> `完了` に更新してください。
- **タスクの追加**: 新しいタスクは、関連するマイルストーンの下に追加してください。大きなタスクは、関係者と合意の上で追加します。
- **完了の定義**: 「完了」とは、実装が完了し、関連するテストが成功した状態を指します。
- **タスクの分割と確認**: 予期せぬ中断リスクを考慮し、タスクは可能な限り短く区切って実装してください。一つの区切りがついたら、コンパイルや簡単な動作確認を行い、問題がなければ進捗を更新（またはコミット）することを推奨します。

---

## 2. 長期マイルストーン

ゴールに向けた高レベルな開発フェーズです。

| マイルストーン | 内容 | ステータス |
| :--- | :--- | :--- |
| **M1: コアロジックの実装完了** | ゲームのセットアップからターン進行、フェーズ遷移、基本的なルール処理まで、バックエンドのロジックを完成させる。 | `完了` |
| **M1.5: アーキテクチャのリファクタリング** | GameCoreとゲーム固有実装の分離を徹底し、設計をクリーンに保つための改善作業。 | `完了` |
| **M2: UIとゲームロジックの接続** | Unity上でゲームの状態（手札、場、リソース等）が可視化され、更新が反映されるようにする。 | `完了` |
| **M3: プレイヤー入力の実装 (UI連携)** | UIを通じて、プレイヤーがゲーム内のアクションを対話形式で行えるようにする。 | `進行中` |
| **M4: カード能力の拡充と自動化** | `AbilityFactory` を拡張し、多様なカード能力をデータ定義から自動生成・実行できるようにする。 | `進行中` |
| **M5: 対戦とテスト** | 2人対戦が可能になり、主要なカードの相互作用が正しく機能することを確認する。バグ修正とパフォーマンス改善を中心に行う。 | `未着手` |

---

## 3. 直近の詳細タスク

### M1: コアロジックの実装
*Note: 基本的なルールは実装完了。残るはカウンターステップの完全実装。*

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-01** | **ゲーム初期化処理 (`WeissGame.SetupGame`)** | Gemini | `完了` |
| **T-02** | **カードデータベースの構築** | Gemini | `完了` |
| **T-03** | **基本フェーズ遷移の実装** | Gemini | `完了` |
| **T-04** | **メインフェイズのアクションループ (`MainPhase.cs`)** | Gemini | `完了` |
| **T-04.5**| **コアロジックの安定化** | Gemini | `完了` |
| **T-05** | **アタックフェイズの基本処理 (`AttackPhase.cs`)** | Gemini | `完了` |
| **T-05.1** | **カウンターステップの実装**: 非ターンプレイヤーが助太刀カードを使用できるロジックを実装する。 | Gemini | `完了` |
| **T-05.2** | **アンコールステップの実装** | Gemini | `完了` |

### M1.5: アーキテクチャのリファクタリング

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-R1** | **ゾーン関連インターフェースの移行**: `IDeckZone`, `IHandZone`など、`GameCore`に誤って配置されているゲーム固有のインターフェースを、`WeissSchwarz`配下に移動する。 | Gemini | `完了` |

### M3: プレイヤー入力の実装 (UI連携)
*Note: `IPlayerController`インターフェースは定義済み。`ConsolePlayerController`はコンソールでのシミュレーション/テスト用。実際のUI連携は`UIGamePlayerController`で行う。*

**M3 - フェーズ1: `UIGamePlayerController`の基盤構築**
| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M3.0** | **`UIGamePlayerController`の作成**: `IWeissPlayerController`を実装するUI連携用コントローラクラスを新規作成する。 | Gemini | `完了` |

**M3 - フェーズ2: UI要素の準備と`UIGamePlayerController`への連携**
*Note: 以下のタスクは、UIチームがUnity Editor上でUI要素を配置し、`UIGamePlayerController`と連携させるためのものです。*

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M3.1** | **マリガン選択UIの実装**: ゲーム開始時に、手札からマリガンするカードをUIで選択できるようにする。 | UIチーム | `完了` |
| **T-M3.2** | **レベルアップカード選択UIの実装**: レベルアップ時、クロック置場のカードからレベル置場に送る1枚をUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.3** | **クロックフェイズ選択UIの実装**: クロックフェイズで、手札からクロック置場に置くカードをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.4** | **メインフェイズ行動選択UIの実装**: メインフェイズで「カードプレイ」「能力起動」「フェイズ終了」をUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.5** | **手札からのカード選択UIの実装**: カードをプレイする際、手札のリストからUIでカードを選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.6** | **クライマックスカード選択UIの実装**: クライマックスフェイズに、プレイするクライマックスカードをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.7** | **アタッカー選択UIの実装**: アタックフェイズに、攻撃するキャラをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.8** | **アタック種類選択UIの実装**: フロントアタックかサイドアタックかをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.9** | **アタック終了確認UIの実装**: 各アタックの後、アタックフェイズを続けるか終了するかをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.10**| **カウンターカード選択UIの実装**: カウンターステップで、使用する助太刀カードをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.11**| **アンコール選択UIの実装**: アンコールステップで、アンコール（標準/特殊）を行うか否かをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.12**| **起動能力の選択UIの実装**: メインフェイズで、複数の起動能力がある場合にどれを使用するかをUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.13**| **任意効果の使用確認UIの実装**: 「～してもよい」と書かれた自動能力が発動した際に、使用するかどうかをUIで確認する仕組みを実装する。 | UIチーム | `未着手` |
| **T-M3.14**| **同時発動した自動能力の解決順選択UIの実装**: 複数の自動能力が同時に発動した場合、プレイヤーが解決する順番をUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.15**| **控え室からのカード選択UIの実装**: カード効果で控え室からカードを回収する際、対象をUIで選択できるようにする。 | UIチーム | `未着手` |
| **T-M3.16**| **汎用的な対象選択UIの実装**: カード効果による対象選択や、公開されたカードからの選択など、汎用的な選択処理をUIで実装する。 | UIチーム | `未着手` |
| **T-M3.17**| **コスト支払いカード選択UIの実装**: コスト支払い（例: 手札破棄）のためのカード選択をUIで実装する。 | UIチーム | `未着手` |


### M4: カード能力の拡充と自動化
*Note: アビリティとコストの解析システムの基盤を構築中。*

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M4.1** | **コスト解析システムの構築**: アビリティテキストからコスト（例: `[(1)]`）を解析する`CostFactory`と、アビリティを生成する`AbilityFactory`を実装する。 | Gemini | `完了` |
| **T-M4.2** | **プレイヤー選択処理の実装**: コスト支払い（例: 手札破棄）のための対話的な選択処理を実装する。 | Gemini | `完了` |
| **T-M4.3** | **`IPlayerController`の拡張**: 汎用的なカード選択メソッド `SelectCards` をインターフェースに追加する。 | Gemini | `完了` |
| **T-M4.4** | **`ConsolePlayerController`の実装**: `IWeissPlayerController`を実装するコンソールクラスを新規作成する。 | Gemini | `完了` |
| **T-M4.5** | **`DiscardCost`の実装**: 手札破棄コストクラスを実装し、`CostFactory`に統合する。 | Gemini | `完了` |
| **T-M4.6** | **アビリティのトリガーと効果の解析**: アビリティテキストからトリガー（【自】【起】【永】）と効果を解析するロジックを`AbilityFactory`に追加する。 | Gemini | `進行中` |
| **T-M4.7** | **効果テキストの解析とIEffectオブジェクトへの変換**: `WeissAbility`の`Description`プロパティから効果テキストを解析し、対応する`IEffect`オブジェクトを生成して`Effects`リストに追加する。(Power Boost, Soul Boost, LookTopAndPlace, ReturnFromWaitingRoomの効果解析を実装済み) | Gemini | `進行中` |

### M2: UIとゲームロジックの接続
*Note: UI表示の基盤となるC#スクリプトは全て実装完了。Unity Editor上でのプレハブ作成と設定はユーザーにより完了済み。*

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M2.1** | **`CardUI.cs` の作成**: 単一カードの表示と状態管理（表裏、レスト等）を行う。 | Gemini | `完了` |
| **T-M2.2** | **`GameView.cs` の作成**: 全てのゾーンUIを統括し、更新を指示する。 | Gemini | `完了` |
| **T-M2.3** | **各ゾーンUIスクリプトの作成**: `HandZoneUI`, `StageZoneUI`, `ClimaxZoneUI`, `LevelZoneUI`, `StockZoneUI`, `ClockZoneUI`, `WaitingRoomUI`, `DeckZoneUI`, `MemoryZoneUI`を実装。 | Gemini | `完了` |

---

## 4. 新規マイルストーン

### M6: UI機能の拡充

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M6.1** | **カード詳細表示UIのC#スクリプト作成 (`CardDetailView.cs`)** | Gemini | `完了` |
| **T-M6.2** | **`GameView.cs` へのカード詳細表示機能統合** | Gemini | `完了` |
| **T-M6.3** | **`CardUI.cs` へのクリックイベント追加** | Gemini | `完了` |
| **T-M6.4** | **Unity Editorでのカード詳細UIプレハブ作成と設定** | UIチーム | `進行中` |

### 実装確認メモ (2026-04-05)
- `T-M3.1` は実装完了。中央モーダル(`DecisionModalController`) + 手札選択(`HandZoneUI`/`CardUI`)でマリガン選択が可能。
- `GameManager` の開始順を修正し、初期手札表示後にマリガン待機へ入る動作に修正済み。
- `CardUI` は選択中カードを赤色表示するよう更新済み。`cardButton` 未割り当て時もクリックを拾えるフォールバックを実装済み。
- `T-M6.4` は `CardDetail.prefab` 参照設定は完了。`CardUIPrefab.cardButton` は推奨設定だが、現状はフォールバックで動作。
- `T-M7.10`（デッキ保存/読み込み）は `DeckEditorManager` 内に保存・読込処理が見当たらず、引き続き `未着手` 判定。

### 次の実装計画 (2026-04-05)
1. **M3 同期入力APIの非同期化設計**
- `IWeissPlayerController` のうち、UI待機が必要な入力 (`ChooseMainPhaseAction`, `AskYesNo` など) を `Task<T>` 化する設計に更新。
- 影響範囲 (`WeissPhases`, `WeissRuleEngine`, `ConsolePlayerController`, `UIGamePlayerController`) を明示し、段階移行できる差分に分割する。
2. **M3 フェーズ実行の非同期化**
- `PhaseBase.Execute` 系の呼び出し経路を非同期対応し、メインフェーズ/能力確認でモーダル待機できるようにする。
- 先行実装は `T-M3.4`（メインフェーズ行動選択）と `T-M3.13`（Yes/No確認）を対象にする。
3. **M3 中央モーダルの拡張**
- `DecisionModalController` の `ShowMainPhaseActionAsync` / `ShowYesNoAsync` を実戦導線に接続。
- モーダル表示中の入力遮断ポリシー（背景レイキャスト有無）を仕様化する。
4. **M3 後続タスク展開**
- `T-M3.8`, `T-M3.9`, `T-M3.11` を同じモーダル基盤へ段階実装する。
- 完了条件は「UI待機」「選択結果反映」「キャンセル時挙動」の3点で統一する。

### 次タスク分解（着手順）
| 分解ID | 内容 | 対象ファイル | 完了条件 |
| :--- | :--- | :--- | :--- |
| **A-M3.1** | `IWeissPlayerController` の部分非同期化（`ChooseMainPhaseAction`, `AskYesNo`） | `Assets/Scripts/WeissSchwarz/Core/IWeissPlayerController.cs` | インターフェース定義が `Task<T>` に更新される |
| **A-M3.2** | `ConsolePlayerController` / `UIGamePlayerController` を新シグネチャへ追従 | `Assets/Scripts/WeissSchwarz/Core/ConsolePlayerController.cs`, `Assets/Scripts/WeissSchwarz/UI/UIGamePlayerController.cs` | コンパイルエラーなく `Task.FromResult/await` で実装される |
| **A-M3.3** | フェーズ実行の async 対応 (`PhaseBase`, `MainPhase` 先行) | `Assets/Scripts/GameCore/Game/PhaseBase.cs`, `Assets/Scripts/WeissSchwarz/Core/WeissPhases.cs` | `MainPhase` の入力待機が `await` で処理される |
| **A-M3.4** | RuleEngine / GameManager の async 実行導線化 | `Assets/Scripts/WeissSchwarz/Core/WeissRuleEngine.cs`, `Assets/Scripts/WeissSchwarz/Core/GameManager.cs` | `ExecuteTurnAsync` を `GameManager` が待機実行する |
| **A-M3.5** | 中央モーダルへの実接続 (`MainPhaseAction`, `YesNo`) | `Assets/Scripts/WeissSchwarz/UI/DecisionModalController.cs`, `Assets/Scripts/WeissSchwarz/UI/GameView.cs` | メインフェイズとYes/Noでモーダル待機が発生する |
| **A-M3.6** | 動作確認とステータス更新 | `docs/TODO_ActionPlan.md` | `T-M3.4`, `T-M3.13` の状態更新と確認メモ記録 |

### 引き継ぎ情報（初見実装者向け）
- **まず読む**: `docs/TODO_ActionPlan.md`（本書）→ `docs/DecisionModal_Setup.md` → `Agent.md`
- **非同期移行の設計書**: 本ドキュメント内「次タスク分解（着手順）」を参照
- **現在の入口クラス**
  - 開始フロー: `Assets/Scripts/WeissSchwarz/Core/GameManager.cs`
  - 対話UI集約: `Assets/Scripts/WeissSchwarz/UI/DecisionModalController.cs`
  - UI入力実装: `Assets/Scripts/WeissSchwarz/UI/UIGamePlayerController.cs`
  - 手札選択: `Assets/Scripts/WeissSchwarz/UI/HandZoneUI.cs`, `Assets/Scripts/WeissSchwarz/UI/CardUI.cs`
- **シーン/プレハブ確認ポイント**
  - `SampleScene` の `GameView.decisionModalController` 参照
  - `Assets/Resources/Prefabs/ModalOverlay.prefab` の Text/Button 参照割り当て
  - `Assets/Resources/Prefabs/CardUIPrefab.prefab` の `cardButton`（推奨）
- **着手前の動作確認**
  - ゲーム開始時に手札が表示される
  - マリガンモーダルでカード選択（赤色表示）と決定ができる
  - ログが `TurnStarted` のみで停止しない

---

## 5. 新規マイルストーン

### M7: デッキ編成機能

**フェーズ1: 基盤構築とデータ連携**

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M7.1** | サンプルJSONをStreamingAssetsに移動 | Gemini | `完了` |
| **T-M7.2** | カードリスト表示UIのC#スクリプト作成 (`DeckEditorManager.cs`, `CardListItem.cs`) | Gemini | `完了` |
| **T-M7.3** | JSONからのカードデータ読み込みと表示ロジック実装 | Gemini | `完了` |
| **T-M7.4** | カード詳細表示機能との連携 | Gemini | `完了` |
| **T-M7.5** | SQLiteへのデータインポート機能実装 | Gemini | `完了` |
| **T-M7.6** | アプリ起動時のデータダウンロードとDB更新処理 | Gemini | `完了` |
| **T-M7.7** | デッキエディタのデータソースをSQLiteに変更 | Gemini | `完了` |

**フェーズ2: コア機能の実装**

| タスク ID | タスク内容 | 担当者 | ステータス |
| :--- | :--- | :--- | :--- |
| **T-M7.8** | **カードの検索・フィルタリング機能実装**: カード名、種類、レベル等でリストを絞り込めるUIとロジックを実装する。 (汎用的な検索基盤クラス `CardQuery`, `CardDatabase` を実装し、`DeckEditorManager` がそれを利用する形に修正) | Gemini | `完了` |
| **T-M7.9** | **デッキへのカード追加・削除機能実装**: カードリストから選択したカードをデッキリストに追加、またはデッキリストから削除するUIとロジックを実装する。(50枚制限、同名カード4枚制限含む) | Gemini | `完了` |
| **T-M7.10**| **デッキの保存・読み込み機能実装**: 編成したデッキをファイルやDBに保存し、次回起動時に読み込めるようにする。 | | `未着手` |
## WeissBattleScene 追加計画 (2026-05-06)

- 対戦準備フェーズ
  - 人間プレイヤーと NPC プレイヤーを `PlayerControlMode` と controller 実装で分離する。
  - 50 枚デッキのロード、デッキ検証、先攻後攻決定、初期手札、マリガンを `WeissBattleScene` の標準開始処理にする。
- カード移動アニメ
  - zone 移動時にカード UI を線形補間で移動させる最小構成のアニメーションを追加する。
  - まずは `CardMoved` イベントを受けて移動演出できる構造を優先し、演出の高度化は後続タスクに分離する。

### 着手済みメモ (2026-05-06)

- `PlayerControlMode` を追加し、人間入力と NPC 自動操作を `Player` / `WeissPlayer` で区別できるようにした。
- `HumanWeissPlayerController` と `SimpleNpcWeissPlayerController` を追加し、`GameManager` の設定から切り替えて `WeissGame` に注入する構造へ変更した。
- `WeissDeckLoader` / `WeissDeckValidator` / `WeissBattleSetupOptions` を追加し、対戦準備フェーズで 50 枚デッキのロード、検証、初期手札作成、マリガン、先攻ランダム決定を行う流れを実装した。
- 既定デッキとして `battle_ready_sample.txt` を追加した。
- カード移動アニメは未着手。次タスクで `CardMoved` イベント起点の UI 演出を追加する。
