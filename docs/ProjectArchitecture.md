# TCG Engine Project

## 概要

このプロジェクトは、Unityで開発されたトレーディングカードゲーム（TCG）制作用の汎用エンジンです。
特定のTCGのルールに依存しないコアシステム (`GameCore`) と、そのコアシステムを基にした具体的なTCGの実装例 (`WeissSchwarz`, `TCGPokemon`) で構成されています。

## 特徴

- **汎用的なコアシステム**: `GameCore`により、様々なカードゲームの基本的な要素（カード、デッキ、ゾーン、フェーズ、アビリティ）を管理できます。
- **拡張性**: 新しいカードゲームを追加する際は、`GameCore` のクラスを継承して、そのゲーム固有のロジックやルールを実装することで容易に拡張できます。
- **実装例**: ヴァイスシュヴァルツとポケモンカードゲームの実装が含まれており、エンジンをどのように利用するかの具体的なサンプルとして役立ちます。

## アーキテクチャ原則

このプロジェクトは、`GameCore`とゲーム固有モジュール（`WeissSchwarz`など）の**関心を完全に分離する**ことを目指しています。

- **`GameCore`の責務**: `Card`, `Player`, `ZoneBase`など、あらゆるカードゲームに共通する、完全に抽象化された概念のみを定義します。`GameCore`は、特定のゲームのルール、用語、UIに一切依存してはいけません。

- **ゲーム固有モジュールの責務**: `GameCore`の基本クラスを継承し、特定のゲームのルール、UI、カードデータ、ゾーンの役割（例：`IDeckZone`, `IStageZone`）などをすべて実装します。例えば、`WeissSchwarz`モジュールはヴァイスシュヴァルツを遊ぶために必要なすべての要素を含みます。

最近実施されたリファクタリングは、この原則を徹底するためのものです。当初`GameCore`に存在した`IDeckZone`や`IStageZone`といったインターフェースは、その「役割」が特定のゲームに依存する可能性があるため、`WeissSchwarz`モジュールに移動されました。これにより、`GameCore`の汎用性がより高まっています。

## ディレクトリ構造

- `Assets/Scripts/GameCore`: TCGの汎用的なコア機能。
- `Assets/Scripts/TCGPokemon`: ポケモンカードゲームの実装例。
- `Assets/Scripts/WeissSchwarz`: ヴァイスシュヴァルツの実装例。
- `Assets/Scenes`: ゲームのメインシーンなどが含まれます。

## ネームスペースと主要クラス（抜粋）

この節では、実際にソース上で使われているネームスペースと代表的なクラス・インターフェースを整理します。

- `TCG.Core` (主に `Assets/Scripts/GameCore` 配下)
	- Card / CardBase<TData> / CardData
	- Player
	- IZone / IZone<TCard> / ZoneBase
	- GameBase / GameState / PhaseBase
	- AbilityBase / IEffect / ITriggerCondition
	- EventBus / GameEventType

- `TCG.Weiss` (ヴァイスシュヴァルツ実装、`Assets/Scripts/WeissSchwarz`)
	- WeissCard : CardBase<WeissCardData>
	- WeissGame : GameBase
	- 独自ゾーンインターフェース（例: `IDeckZone`, `IStageZone`）およびその実装

（将来的な拡張）`TCG.TCGPokemon` 等の別ゲーム実装も同様の命名規則で配置されます。

## 代表的なクラス継承・関係の説明

ここでは、ドキュメントを読んだだけで構造が分かるように、主要な継承関係をテキストで示します。

1) カード系

	 - Card (非ジェネリック基底)
		 - CardBase<TData> : Card where TData : CardData
			 - WeissCard : CardBase<WeissCardData>

	 説明: `Card` は物理的な状態（所有者、ゾーン、タップ状態等）を持つ最も基本的な型です。
	 `CardBase<TData>` はカード固有データ（`CardData` 派生型）を持つためのジェネリック基底で、個々のゲーム実装はこれを継承して具体化します。

2) ゾーン系

	 - IZone (インターフェース)
		 - IZone<TCard> : IZone
		 - ZoneBase : IZone

	 説明: ゾーン（手札、山札、場など）の共通操作は `IZone` に定義され、`ZoneBase` が基本機能（カード追加・削除、シャッフルなど）を提供します。
	 ゲーム固有の役割を厳密に分離したい場合、`IDeckZone` や `IHandZone` のようなより具体的なインターフェースはゲーム実装側（`WeissSchwarz`）に置かれています。

3) ゲーム進行

	 - GameBase (抽象)
		 - WeissGame : GameBase

	 説明: 全体のターン進行やフェーズ管理の骨組みは `GameBase` が担います。ゲーム固有ルール（勝利条件、フェーズ間の特殊処理等）は `WeissGame` のように継承先で実装します。

## ドキュメントのギャップと追加提案

現在の `docs` は設計方針（責務分離、データ駆動の能力設計）をよく説明していますが、以下の追記を推奨します:

- クラス図（PlantUML あるいは Mermaid）を一つ追加し、主要クラス間の継承/依存関係を視覚化する。
- クラス図（PlantUML あるいは Mermaid）を一つ追加し、主要クラス間の継承/依存関係を視覚化する。

### 図: クラス図 (PlantUML)

ソースツリーに PlantUML ファイルを追加しました:

- `docs/diagrams/architecture.puml`

このファイルはリポジトリ内で PlantUML を使って描画できます。ローカルで SVG/PNG に変換するには、PlantUML の CLI や VSCode の PlantUML 拡張を使ってください。簡単な例:

```powershell
# PlantUML.jar を使って PNG を生成する例（Java 実行環境が必要）
java -jar plantuml.jar docs\diagrams\architecture.puml -tpng
```

あるいは、VSCode の PlantUML 拡張で `.puml` ファイルを開くとプレビューできます。
- `Assets/Scripts` 配下のディレクトリツリー（深さ 2〜3）を自動生成して貼る（README に簡易表示）。
- 各主要インターフェース（`IZone`, `IEffect`, `ITriggerCondition` など）について、責務と主要メソッドのシグネチャ抜粋をまとめる。

上記は低リスクで価値が高い改善です。必要なら私の方で PlantUML ソースや更新パッチを作成します。

## 技術スタック

- **エンジン**: Unity `2022.3.12f1`
- **言語**: C#

## 実行方法

1. このプロジェクトをUnity Editor (`2022.3.12f1`以降)で開きます。
2. `Assets/Scenes/SampleScene.unity` を開きます。
3. Unity Editorの上部にある再生ボタンをクリックして、ゲームを実行します。

## 今後の展望（提案）

- UI/UXの改善
- ネットワーク対戦機能の実装
- より多くのTCG実装例の追加
- デッキ構築機能
