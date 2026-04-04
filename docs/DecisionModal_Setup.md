# Decision Modal セットアップ手順

この手順は、`DecisionModalController` を使って「中央モーダル単一インスタンス」を運用するための設定メモです。

## 1. シーン配置
- `SampleScene` の Canvas 配下に、中央モーダル用プレハブを 1 つ配置する。
- ルートに `DecisionModalController` をアタッチする。
- 初期表示は非表示にする（`modalRoot` を非アクティブ）。

## 2. Inspector 割り当て（DecisionModalController）
- `modalRoot`
- `titleText`
- `messageText`
- `primaryButton`, `primaryButtonText`
- `secondaryButton`, `secondaryButtonText`
- `tertiaryButton`, `tertiaryButtonText`（任意だが推奨）

## 3. Inspector 割り当て（GameView）
- `decisionModalController` に上記モーダルの `DecisionModalController` を割り当てる。

## 4. CardUI 側の必須設定
- `Assets/Resources/Prefabs/CardUIPrefab.prefab` の `CardUI.cardButton` を割り当てる。
- （任意）`CardUI.selectedOverlayObject` を割り当てる。
  - 未設定でも選択機能は動作するが、見た目のハイライトは出ない。
  - 現在は `cardButton` 未割り当て時のクリックフォールバックを実装済み。ただし将来的な保守のため割り当てを推奨。

## 5. 現在実装済みのモーダル機能
- `ShowMulliganAsync`（マリガン選択）
- `ShowYesNoAsync`（2択）
- `ShowMainPhaseActionAsync`（3択。`tertiaryButton` がない場合 `UseAbility` は未表示）

## 6. 注意
- `ChooseMainPhaseAction` / `AskYesNo` などの同期インターフェースに対して「UI待機」を完全対応するには、フェーズ実行の非同期化が必要。
- 現在はマリガン（もともと `Task` ベース）を優先して中央モーダル化済み。
- `ModalOverlay` の背景が手札クリックを遮る場合は、背景Imageの `Raycast Target` 設定を確認する。
