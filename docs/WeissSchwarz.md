# Weiss Schwarz モジュール開発ガイド

このドキュメントでは、本プロジェクトにおける「ヴァイスシュヴァルツ」実装 (`Assets/Scripts/WeissSchwarz`) 固有の仕様、特にカードデータの管理と開発フローについて解説します。

## 📂 ディレクトリ構成とデータ管理

Weissモジュールでは、カードデータを **JSON** で管理し、実行時はパフォーマンスと管理のしやすさのために **SQLite** データベースを使用するフローを採用しています。

| パス | 説明 |
| --- | --- |
| `Assets/StreamingAssets/WeissSchwarz/cards.json` | **マスターデータ（編集用）**。<br>開発者はこのファイルを編集してカードを追加・修正します。 |
| `Assets/StreamingAssets/WeissSchwarz/cards.db` | **アプリ配布用データベース**。<br>JSONから自動生成されるファイルです。手動で編集しないでください。 |
| `Assets/Scripts/WeissSchwarz/Data/CardDataImporter.cs` | JSON ⇔ SQLite の変換ロジック。 |
| `Assets/Scripts/WeissSchwarz/Core/WeissCardDatabase.cs` | ゲーム実行時のデータロード処理。 |

---

## 🛠 開発ワークフロー

### 1. カードデータの追加・修正方法

1. **JSONの編集**:
   `Assets/StreamingAssets/WeissSchwarz/cards.json` を開き、カードデータを追記・修正してください。

   **フォーマット例:**
   ```json
   {
     "cards": [
       {
         "cardCode": "HOL/W104-001",
         "name": "未来へ一緒に ときのそら",
         "set": "ホロライブプロダクション Vol.2",
         "rarity": "RR",
         "cardType": "キャラクター",
         "color": "Yellow",
         "level": 0,
         "cost": 0,
         "power": 1500,
         "soul": 1,
         "trigger": "None",
         "traits": ["ホロライブ", "0期生"],
         "text": ["【永】...", "【自】..."],
         "flavor": "みんなー！元気ー？ときのそらです！"
       }
     ]
   }
   ```

2. **データベースの更新 (必須)**:
   Unityエディタ上で、以下のメニューを実行します。
   
   👉 **`Tools > Weiss Schwarz > Generate DB from JSON`**
   
   これを行うと、`cards.json` の内容が `cards.db` に変換されます。
   ※ この手順を飛ばすと、ゲーム実行時に変更が反映されません。

3. **実行確認**:
   ゲームを実行します。
   - 初回起動時、またはDBファイルが存在しない場合、`StreamingAssets` のDBが `PersistentDataPath`（セーブデータ領域）にコピーされます。

### 2. プログラムからのデータアクセス

カードデータは `WeissCardDatabase` クラスを通じてアクセスします。これは `TCG.Core` の `CardDatabase` を継承したクラスです。

```csharp
// シングルトンから全カードリストを取得
var allCards = WeissCardDatabase.Instance.GetAllCards();

// 条件で検索（LINQなどを使用）
var myCard = allCards.FirstOrDefault(c => c.CardCode == "HOL/W104-001");
```

---

## ⚠️ 注意事項

- **ScriptableObjectは不使用**: 以前のバージョンで使用していた `ScriptableObject` によるカード管理は廃止されました。現在はSQLite直接読み込み方式です。
- **DBファイルのGit管理**: `cards.db` はバイナリファイルですが、アプリ配布に必要なためGitにコミットしてください（またはCIでJSONから生成するフローを構築してください）。