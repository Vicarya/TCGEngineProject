# UIチーム向け作業指示書

このドキュメントは、TCGエンジンプロジェクトにおけるUIチームの皆様がUnity Editor上で行うべき具体的な作業をまとめたものです。主に、プレイヤー入力のUI連携（M3）とカード詳細表示UIのセットアップ（T-M6.4）に関するタスクが含まれます。

---

## 1. カード詳細表示UIのセットアップ (T-M6.4)

**目的:** プレイヤーがカードをクリックした際に、そのカードの詳細情報が表示されるUIをUnity Editor上で作成し、既存のC#スクリプトと連携させます。

**担当:** UIチーム
**ステータス:** 未着手

**作業内容:**

1.  **`CardDetailView` プレハブの作成:**
    *   Unity Editorで新しいUI Canvasを作成するか、既存のCanvas内に、カード詳細表示用のUIパネル（例: `Panel_CardDetail`）を作成します。このパネルは、通常は画面中央にオーバーレイ表示されることを想定しています。
    *   このパネル内に、以下のUI要素を配置してください。
        *   **カード画像表示用 `Image`:** 左側に配置し、カードのイラストを表示します。
        *   **詳細テキスト表示用 `TextMeshPro - Text`:** 右側に配置し、カードのテキスト情報を表示します。
        *   **閉じるボタン `Button`:** UIを閉じるためのボタンを配置します。
    *   このUIパネルのルートGameObjectに、**`CardDetailView.cs`** スクリプトをアタッチします。
    *   `CardDetailView.cs` スクリプトのインスペクターで、以下のフィールドに、対応するUI要素をドラッグ＆ドロップで割り当ててください。
        *   `Card Image View`: 作成した `Image` コンポーネント
        *   `Card Detail Text`: 作成した `TextMeshPro - Text` コンポーネント
        *   `Close Button`: 作成した `Button` コンポーネント
    *   このUIパネルをプレハブとして保存します（例: `Assets/Resources/Prefabs/CardDetailView.prefab`）。

2.  **`GameView` への `CardDetailView` プレハブの登録:**
    *   シーン内の `GameView` オブジェクト（通常はCanvasのルートなど、`GameView.cs` スクリプトがアタッチされているGameObject）を選択します。
    *   `GameView.cs` スクリプトのインスペクターに新しく追加された `Card Detail View` フィールドに、上記で作成した `CardDetailView.prefab` をドラッグ＆ドロップで割り当ててください。

3.  **`CardUI` プレハブへの `Button` コンポーネントの追加と設定:**
    *   既存の `CardUI` プレハブ（例: `Assets/Resources/Prefabs/CardUIPrefab.prefab`）を開きます。
    *   `CardUI` スクリプトがアタッチされているGameObject（またはその子GameObject）に `Button` コンポーネントを追加します。このボタンは、カード全体をカバーするように設定してください。
    *   `CardUI.cs` スクリプトのインスペクターで、新しく追加された `Card Button` フィールドに、この `Button` コンポーネントをドラッグ＆ドロップで割り当ててください。

---

## 2. プレイヤー入力のUI連携 (M3 - フェーズ2)

**目的:** プレイヤーがゲーム内でアクションを行うためのUI要素をUnity Editor上で作成し、`UIGamePlayerController` と連携させます。これにより、ゲームロジックがUIからの入力を受け取れるようになります。

**担当:** UIチーム
**ステータス:** 未着手 (各タスク)

**共通事項:**
*   各UI要素は、`UIGamePlayerController` が適切なタイミングで表示/非表示を切り替えられるように、`GameObject.SetActive(true/false)` で制御できる状態にしてください。
*   `UIGamePlayerController` は、UIからの選択結果を受け取るためのメソッド（例: `ConfirmMulligan(List<WeissCard> selectedCards)`）を提供します。UI要素のイベント（ボタンクリックなど）からこれらのメソッドを呼び出すように設定してください。

**各タスクの詳細:**

| タスク ID | タスク内容 | 具体的なUI要素と連携方法の例 |
| :--- | :--- | :--- | :--- |
| **T-M3.1** | **マリガン選択UIの実装** | 手札のカードを一覧表示し、マリガンしたいカードを選択（チェックボックスなど）し、「マリガン確定」ボタンで `UIGamePlayerController.ConfirmMulligan()` を呼び出す。 |
| **T-M3.2** | **レベルアップカード選択UIの実装** | クロック置場のカードを一覧表示し、レベル置場に送る1枚を選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.3** | **クロックフェイズ選択UIの実装** | 手札からクロック置場に置くカードを選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.4** | **メインフェイズ行動選択UIの実装** | 「カードプレイ」「能力起動」「フェイズ終了」などの選択肢をボタンで表示するUI。各ボタンクリックで `UIGamePlayerController.ChooseMainPhaseAction()` に対応するアクションを渡す。 |
| **T-M3.5** | **手札からのカード選択UIの実装** | カードをプレイする際、手札のリストからUIでカードを選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.6** | **クライマックスカード選択UIの実装** | クライマックスフェイズに、プレイするクライマックスカードをUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.7** | **アタッカー選択UIの実装** | アタックフェイズに、攻撃するキャラをUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.8** | **アタック種類選択UIの実装** | フロントアタックかサイドアタックかを選択するボタンUI。選択後、`UIGamePlayerController.ChooseAttackType()` を呼び出す。 |
| **T-M3.9** | **アタック終了確認UIの実装** | 各アタックの後、アタックフェイズを続けるか終了するかをUIで選択するUI。選択後、`UIGamePlayerController.ChooseToEndAttack()` を呼び出す。 |
| **T-M3.10**| **カウンターカード選択UIの実装** | カウンターステップで、使用する助太刀カードをUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.11**| **アンコール選択UIの実装** | アンコールステップで、アンコール（標準/特殊）を行うか否かをUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.12**| **起動能力の選択UIの実装** | メインフェイズで、複数の起動能力がある場合にどれを使用するかをUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.13**| **任意効果の使用確認UIの実装** | 「～してもよい」と書かれた自動能力が発動した際に、使用するかどうかをUIで確認するUI（「はい/いいえ」ボタンなど）。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.14**| **同時発動した自動能力の解決順選択UIの実装** | 複数の自動能力が同時に発動した場合、プレイヤーが解決する順番をUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.15**| **控え室からのカード選択UIの実装** | カード効果で控え室からカードを回収する際、対象をUIで選択するUI。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.16**| **汎用的な対象選択UIの実装** | カード効果による対象選択や、公開されたカードからの選択など、汎用的な選択処理をUIで実装する。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |
| **T-M3.17**| **コスト支払いカード選択UIの実装** | コスト支払い（例: 手札破棄）のためのカード選択をUIで実装する。選択後、`UIGamePlayerController` の対応するメソッドを呼び出す。 |