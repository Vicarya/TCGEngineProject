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
- `ChooseMainPhaseAction` / `AskYesNo` はすでに `Task` ベースへ移行済みで、`GameManager` からの非同期導線も通っている。
- ただし `ChooseCardFromHand` など多くの UI 入力 API は未実装で、`WeissRuleEngine` 側にも `AskYesNo(...).GetAwaiter().GetResult()` が残っているため、UI待機はまだ完全ではない。
- 現在の中央モーダル実運用は、マリガン、メインフェイズ行動選択、Yes/No 確認まで。
- `ModalOverlay` の背景が手札クリックを遮る場合は、背景Imageの `Raycast Target` 設定を確認する。
