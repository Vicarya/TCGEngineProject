using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// ネットワーク経由で画像をロードするための汎用ユーティリティクラス。
    /// </summary>
    public static class ImageLoader
    {
        /// <summary>
        /// 指定されたURLから画像を非同期でロードし、対象のImageコンポーネントに設定します。
        /// </summary>
        /// <param name="url">画像のURL。</param>
        /// <param name="targetImage">画像を表示するImageコンポーネント。</param>
        public static IEnumerator LoadImage(string url, Image targetImage)
        {
            // URLが無効、またはターゲットが存在しない場合は何もしない
            if (string.IsNullOrEmpty(url) || targetImage == null)
            {
                yield break;
            }

            // 画像のダウンロードリクエストを作成
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                // 一部のサーバー（公式画像サーバーなど）ではUser-Agentがないと404を返すため、標準的なブラウザのUAを設定
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // ダウンロード成功時、テクスチャを取得してスプライトを生成
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (texture != null)
                    {
                        // 中心をピボットとするスプライトを作成
                        Sprite sprite = Sprite.Create(
                            texture, 
                            new Rect(0, 0, texture.width, texture.height), 
                            Vector2.one * 0.5f
                        );

                        // 非同期処理中にターゲットが破棄されていないか確認してセット
                        if (targetImage != null)
                        {
                            targetImage.sprite = sprite;
                        }
                    }
                }
                else
                {
                    Debug.LogError($"ImageLoader: 画像のロードに失敗しました ({url}): {request.error}");
                }
            }
        }
    }
}
