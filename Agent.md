# Agent.md

最終更新: 2026-04-04

## 1. 目的（Mission）
このプロジェクトの開発者/エージェントは、設計ルールを守りながら、`docs/TODO_ActionPlan.md` を正本としてタスクを前進させる。

## 2. 前提（Context）
- プロジェクト: Unity製のTCG汎用エンジン
- 現在のサンプル実装: Weiss Schwarz
- 主要コード領域:
  - `Assets/Scripts/GameCore`（共通エンジン層）
  - `Assets/Scripts/WeissSchwarz`（ゲーム固有層）
  - `Assets/Scripts/WeissSchwarz/UI`（UI連携）
  - `Assets/Scripts/WeissSchwarz/Data`（DB/Importer/Repository）
- 仕様・計画ドキュメントは `docs/` 配下に集約

## 3. 正本ドキュメント（優先順位）
1. `docs/TODO_ActionPlan.md`
2. `docs/ProjectArchitecture.md`
3. `docs/ProjectOverview.md`
4. `docs/WeissSchwarz.md`
5. `docs/UI_Team_Instructions.md`
6. `docs/TODO_Handover.md`（背景確認用の旧資料）

## 4. クイックスタート
- Unity: `2022.3.12f1` 以降
- 実行シーン: `Assets/Scenes/SampleScene.unity`
- 実行方法: Unity Editor の Play

## 5. アーキテクチャガードレール

### 5.1 依存方向（厳守）
`UI (TCG.Weiss.UI) -> TCG.Weiss.* -> TCG.Core.*`

禁止:
- `TCG.Core` から `TCG.Weiss` を参照する
- ゲーム固有ロジックを `GameCore` に入れる
- `TCG.Core` に Unity 依存（`MonoBehaviour` など）を入れる

### 5.2 責務分離（厳守）
- `GameCore`: 抽象ルール、共通モデル、共通基盤
- `WeissSchwarz`: Weiss固有ルール/フェーズ/能力
- `UI`: 表示、入力、コントローラ接続
- `Data`: Importer/Repository/RuntimeStore

### 5.3 DB方針（厳守）
- Core側は抽象リポジトリのみ（`CardRepositoryBase<TCardData>`）
- Weiss側で具体リポジトリ（マッピング/スキーマ/UPSERT）を実装
- JSON -> SQLite 変換は Importer に集約

### 5.4 命名規則
- ルート: `TCG`
- 共通層: `TCG.Core.*`
- ゲーム固有層: `TCG.<GameName>.*`（PascalCase）

## 6. タスク進行フロー（Codex標準）
1. `TODO_ActionPlan` で対象タスクID/状態を確認
2. 対象層（UI / Weiss / Core / Data）を明確化
3. 受け入れ条件を短く定義
4. 既存設計に沿った最小差分で実装
5. 依存方向・責務分離を再チェック
6. 検証手順と結果を記録
7. 状態変更があれば `TODO_ActionPlan` を更新

## 7. 着手前チェックリスト
- [ ] タスクIDと現ステータスを確認した
- [ ] 対象層を特定した
- [ ] 依存方向に違反しない
- [ ] 受け入れ条件を定義した
- [ ] Unity Editor 手作業の有無を確認した
- [ ] 既存実装との重複がない

## 8. 完了条件（Definition of Done）
- 要件を満たしている
- 依存方向/責務分離に違反していない
- 変更理由と影響範囲を説明できる
- 必要なドキュメント更新が完了している
- `docs/TODO_ActionPlan.md` と実装状態が一致している

## 9. 引き継ぎテンプレート
```text
Task ID:
目的:
受け入れ条件:
変更ファイル:
変更層(UI/Weiss/Core/Data):
依存方向チェック結果:
検証手順/結果:
未対応事項:
```

## 10. 現在のフォーカス（2026-04-04時点）
- 完了済み: M1, M1.5, M2, M6（コード側）, M7 フェーズ1/2の大半
- 進行中: M4（能力/効果テキスト解析の拡張）
- 未着手の主項目:
  - M3 フェーズ2（UIチームのUnity Editor作業）
  - M6.4（カード詳細UIプレハブ設定）
  - M7.10（デッキ保存/読み込み）

## 11. 禁止事項
- 旧TODO資料を正本として更新しない
- 層を跨ぐ安易な直接参照を追加しない
- DBアクセスを複数箇所へ散在させない
- 都合でドメインロジックをUI層へ移さない

## 12. 文字化け対策（予防・検知・復旧）

### 12.1 予防
- テキストファイルは原則 `UTF-8`（BOMなし）で保存する。
- ファイル編集は `apply_patch` または「明示的に `encoding='utf-8'` を指定した処理」を使う。
- PowerShellで確認する場合は `Get-Content -Encoding utf8` を優先する。
- 文字コード変換が発生しやすい経路（端末依存の貼り付け・暗黙エンコード）を避ける。

### 12.2 検知
- `???` 置換や `U+FFFD (Replacement Character)` の混入がないかを確認する。
- 日本語文書なのに日本語文字数が極端に少ない場合は文字化けを疑う。

### 12.3 復旧
1. まず正本（本ファイル/該当ドキュメント）を `UTF-8` で再読込する。
2. 破損があれば、正しい内容を `UTF-8` 明示で再書き込みする。
3. 再発防止のため、編集経路と原因（どの操作で壊れたか）を記録する。
