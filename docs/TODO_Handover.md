# TCG Engine - TODO引き継ぎドキュメント

最終更新: 2026-05-24

## 運用ルール
- このドキュメントは**作業ごとに更新する運用資料**として扱う。
- 実装完了・仕様変更・設計変更があった場合、同日の更新履歴を必ず追記する。
- 詳細タスク管理は `docs/TODO_ActionPlan.md` を併用する。

## 概要

このドキュメントは、TCG Engineプロジェクトの「今どこまで進んでいて、次に何をやるか」を初見実装者向けに要約した引き継ぎ資料です。

## 現状のアーキテクチャ

- **`GameCore`**: TCG汎用基盤（カード/ゾーン/フェーズ/EventBus）。
- **`WeissSchwarz`**: ゲーム固有実装（ルール・フェーズ・能力・UI）。
- **ゲーム進行**: `GameManager` -> `WeissGame` -> `WeissRuleEngine` -> `WeissPhaseFactory`。
- **UI入力設計**: `IWeissPlayerController` 経由。  
  現在はマリガンに加え、メインフェイズ行動選択と Yes/No モーダルの基盤まで接続済み。

## 現在の進捗（2026-05-24時点）
- マリガンUIは実装済み（手札表示・カード選択・決定/スキップ）。
- `ChooseMainPhaseAction` / `ChooseMulliganCards` / `AskYesNo` は `Task` 化済みで、`GameManager` から `ExecuteTurnAsync` まで非同期導線が通っている。
- `ChooseCardFromHand` は `HandZoneUI` の共通選択セッション経由で UI 待機できるようになり、クロックフェイズとメインフェイズの手札選択は動作する。
- `Player1` は `HumanWeissPlayerController`、`Player2` は `SimpleNpcWeissPlayerController` を経由して構成できる。
- `CardUI` は選択中を赤色表示。`cardButton` 未割り当て時のクリックフォールバックあり。
- `DecisionModalController` にはマリガン、メインフェイズ行動選択、Yes/No の表示 API がある。
- ただし `UIGamePlayerController` のカード選択系メソッドが多数未実装で、`WeissRuleEngine` には `AskYesNo(...).GetAwaiter().GetResult()` が残っているため、UI待機はまだ完全ではない。
- カード移動アニメは `CardMoved` 起点の直線補間までは入っているが、2 段モーションや表裏切替タイミング制御は未実装。

## 次の実装タスク（着手順）
1. `ChooseAbilityToActivate` を実装し、メインフェイズの 3 ルートを完結させる。
2. `WeissRuleEngine` の `AskYesNo(...).GetAwaiter().GetResult()` を撤去し、能力解決キューを async 化する。
3. アタック種類、アタック継続確認、アンコールを同じモーダル基盤へ展開する。
4. `ChooseClimaxFromHand`、`ChooseLevelUpCard`、複数枚選択系 UI を共通選択基盤の上に拡張する。
5. カード移動アニメを「山札 -> 手元見切れ位置 -> 目的地」の 2 段モーションへ拡張する。
6. `DeckEditorManager` にデッキ保存/読込を追加する。

## 初見実装者の確認ポイント
- `SampleScene` の `GameView.decisionModalController` が設定されているか。
- `ModalOverlay.prefab` の Text/Button が `DecisionModalController` に割り当て済みか。
- `CardUIPrefab` の `cardButton` は可能なら割り当てる（未設定でもフォールバック動作）。
- モーダル背景の `Raycast Target` が手札クリックを阻害していないか。

## 主要参照ドキュメント
- 詳細タスク: `docs/TODO_ActionPlan.md`
- モーダル設定: `docs/DecisionModal_Setup.md`
- 非同期移行手順: `docs/TODO_ActionPlan.md` の「次タスク分解（着手順）」

## 更新履歴
- 2026-04-05: 本ドキュメントを「旧資料」扱いから「現行引き継ぎ資料」へ運用変更。最新進捗・次タスク・確認ポイントを更新。
- 2026-05-24: 実装監査に合わせて、M3 の進捗、未実装項目、カード移動アニメの現状と次タスクを更新。

---

# Repository Pattern (SQLite)

更新日: 2026-03-29

## 目的
- Core側でDB実装の共通化を行い、タイトル側は例外処理のみ実装する。

## 基本パターン
1. モデルクラスを定義する（public fieldベース）。
2. `AutoSqliteCardRepositoryBase<T>` を継承してRepositoryを作る。
3. コンストラクタで `tableName` と `primaryKeyColumn` を指定する。
4. 必要に応じて以下だけoverrideする。
   - `OnEntityLoaded`: Runtime既定値補完
   - `OnSchemaEnsured`: 旧列名からの移行や特殊補正

## 型推論ルール（Auto）
- `string -> TEXT`
- `int/long/short/byte/bool -> INTEGER`
- `float/double/decimal -> REAL`
- `List<string> -> TEXT(JSON)`
- 上記以外は永続化対象外

## 例外実装の考え方
- 原則: DB列名 = モデル名
- 例外: 旧スキーマ互換、特殊型変換、Runtime補完
- 例外はタイトル側Repositoryに閉じる

