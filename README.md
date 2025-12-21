# TCG Engine Project

## 概要
【開発中】
このプロジェクトは、Unityで開発されたトレーディングカードゲーム（TCG）制作用の汎用エンジンです。

プロジェクトに関する詳細なドキュメントは `docs` フォルダに集約されています。

## ドキュメント一覧

*   **[プロジェクトアーキテクチャ](./docs/ProjectArchitecture.md)**
    *   このプロジェクトの元々のREADMEです。技術スタックやディレクトリ構造について記載されています。

*   **[プロジェクト概要と設計構造](./docs/ProjectOverview.md)**
    *   `GameCore` と `WeissSchwarz` の実装から分析した、プロジェクトの設計思想やコンポーネントの詳細な解説です。

*   **[開発アクションプラン (TODO)](./docs/TODO_ActionPlan.md)**
    *   対戦シミュレーション実現に向けた、具体的なマイルストーンとタスクリストです。開発に着手する際はこちらを参照してください。

*   **[引き継ぎ用TODO (旧)](./docs/TODO_Handover.md)**
    *   初期のTODOリストです。現在は `TODO_ActionPlan.md` に統合されていますが、過去の経緯として残しています。

## 実行方法

1. このプロジェクトをUnity Editor (`2022.3.12f1`以降)で開きます。
2. `Assets/Scenes/SampleScene.unity` を開きます。
3. Unity Editorの上部にある再生ボタンをクリックして、ゲームを実行します。

## プロジェクト構造の簡易ガイド

このリポジトリは、ドメインごとに責務を分離した2層構造になっています。詳細は `docs/ProjectArchitecture.md` を参照してくださいが、簡単にポイントをまとめます。

- コア（汎用）: `Assets/Scripts/GameCore` —
    - ネームスペース: `TCG.Core`
    - 役割: カード、プレイヤー、ゾーン、ゲームフロー、イベントなど、特定のTCGに依存しない抽象概念を提供
- ゲーム固有実装: `Assets/Scripts/WeissSchwarz`, `Assets/Scripts/TCGPokemon` —
    - ネームスペース例: `TCG.Weiss`（ヴァイスシュヴァルツ実装）
    - 役割: `GameCore` の抽象クラス・インターフェースを継承/実装し、ルール・ゾーンの具体的な振る舞いを定義

主要な継承/実装関係（抜粋）:

- Card (TCG.Core)
    - CardBase<TData> : Card (汎用カードのジェネリック基底)
- IZone (TCG.Core)
    - ZoneBase : IZone（ゾーンの基本実装）
- GameBase (TCG.Core)
    - WeissGame : GameBase（ヴァイス固有のゲーム進行管理）

ドキュメントの補足やクラス図が必要であれば追って追加できます。READMEではまずは参照先と概要を分かりやすく記載しています。
